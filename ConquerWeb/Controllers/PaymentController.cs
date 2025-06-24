using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Web;
using System;
using ConquerWeb.Services;
using System.Globalization;
using ConquerWeb.Models;

namespace ConquerWeb.Controllers
{
    [Route("paymentshandler")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly IHttpClientFactory _httpClientFactory;

        public PaymentController(DatabaseHelper dbHelper, IHttpClientFactory httpClientFactory)
        {
            _dbHelper = dbHelper;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost]
        public async Task<IActionResult> Receive()
        {
            string logFilePath = "ipn_receive_log.txt";
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            await System.IO.File.AppendAllTextAsync(logFilePath, $"[{timestamp}] IPN Receive method entered.\n");

            string requestBody;
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.ASCII))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            await System.IO.File.AppendAllTextAsync(logFilePath, $"[{timestamp}] IPN Request Body: {requestBody}\n");

            _dbHelper.LogError($"IPN Received: {requestBody}");

            await VerifyAndProcessIPN(requestBody);

            return Ok();
        }

        private async Task VerifyAndProcessIPN(string requestBody)
        {
            string verificationResponse = string.Empty;
            try
            {
                var client = _httpClientFactory.CreateClient();
                // Consider reading this BaseAddress from IConfiguration for flexibility (sandbox/live)
                client.BaseAddress = new Uri("https://www.paypal.com/");

                var content = new StringContent($"cmd=_notify-validate&{requestBody}", Encoding.ASCII, "application/x-www-form-urlencoded");
                var response = await client.PostAsync("cgi-bin/webscr", content);

                response.EnsureSuccessStatusCode();

                verificationResponse = await response.Content.ReadAsStringAsync();
                _dbHelper.LogError($"IPN Verification Response: {verificationResponse} - IPN Body: {requestBody}");
            }
            catch (Exception ex)
            {
                _dbHelper.LogError($"IPN Verification Error: {ex.Message} - IPN Body: {requestBody}");
                return;
            }

            if (verificationResponse.Equals("VERIFIED", StringComparison.OrdinalIgnoreCase))
            {
                var paypalObjs = ParseIpnBody(requestBody);

                string receiverEmail = HttpUtility.UrlDecode(paypalObjs.GetValueOrDefault("receiver_email", string.Empty));
                string paymentStatus = paypalObjs.GetValueOrDefault("payment_status", string.Empty);
                string txnId = paypalObjs.GetValueOrDefault("txn_id", string.Empty);
                string itemNumber = paypalObjs.GetValueOrDefault("item_number", string.Empty);
                string itemName = HttpUtility.UrlDecode(paypalObjs.GetValueOrDefault("item_name", string.Empty));
                string currency = paypalObjs.GetValueOrDefault("mc_currency", string.Empty);
                string payerEmail = HttpUtility.UrlDecode(paypalObjs.GetValueOrDefault("payer_email", string.Empty));

                decimal mcGross;
                if (!decimal.TryParse(paypalObjs.GetValueOrDefault("mc_gross", "0.00"), NumberStyles.Any, CultureInfo.InvariantCulture, out mcGross))
                {
                    _dbHelper.LogError($"IPN Error: Could not parse mc_gross. Value: {paypalObjs.GetValueOrDefault("mc_gross", "")}");
                    return;
                }

                int? vipDaysToSave = null;
                int? dbScrollsToSave = null;

                Product purchasedProduct = null;
                int parsedItemNumber;
                if (int.TryParse(itemNumber, out parsedItemNumber))
                {
                    purchasedProduct = _dbHelper.GetProductById(parsedItemNumber);
                    if (purchasedProduct != null)
                    {
                        dbScrollsToSave = purchasedProduct.DBScrolls;
                    }
                }

                if (paymentStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase) &&
                    receiverEmail.Equals("pakjulia94@yandex.ru", StringComparison.OrdinalIgnoreCase) && // REPLACE WITH YOUR PAYPAL BUSINESS EMAIL!
                    purchasedProduct != null && purchasedProduct.ProductPrice == mcGross)
                {
                    try
                    {
                        DateTime paymentDate;
                        if (!DateTime.TryParseExact(paypalObjs.GetValueOrDefault("payment_date", ""), "HH:mm:ss MMM dd,yyyy PST", CultureInfo.InvariantCulture, DateTimeStyles.None, out paymentDate))
                        {
                            paymentDate = DateTime.UtcNow;
                            _dbHelper.LogError($"IPN: payment_date parsing error. Original: {paypalObjs.GetValueOrDefault("payment_date", "")}. UTC Now used.");
                        }

                        _dbHelper.SavePaymentRecord(
                            characterName: itemName,
                            currency: currency,
                            amount: mcGross,
                            email: payerEmail,
                            vipDays: vipDaysToSave,
                            dbScrolls: dbScrollsToSave,
                            payDate: paymentDate,
                            paymentscol: txnId
                        );

                        if (dbScrollsToSave.HasValue)
                        {
                            _dbHelper.UpdateCharacterDBScrolls(itemName, dbScrollsToSave.Value);
                        }
                        else
                        {
                            _dbHelper.LogError($"IPN Success, but DBScrolls value not determined (dbScrollsToSave is null). TXN ID: {txnId}");
                        }

                        _dbHelper.LogError($"IPN Success: New payment record created and DBScrolls updated. TXN ID: {txnId}");
                    }
                    catch (Exception ex)
                    {
                        _dbHelper.LogError($"IPN Processing Error (Database Update): {ex.Message} - TXN ID: {txnId} - Body: {requestBody}");
                    }
                }
                else
                {
                    _dbHelper.LogError($"IPN Processing Failed: TXN ID: {txnId}, Status: {paymentStatus}, Receiver Email: {receiverEmail}, Item ID: {itemNumber}, Amount: {mcGross}");
                    if (!paymentStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                        _dbHelper.LogError($"IPN Error: Payment status is not Completed: {paymentStatus}");
                    if (!receiverEmail.Equals("pakjulia94@yandex.ru", StringComparison.OrdinalIgnoreCase))
                        _dbHelper.LogError($"IPN Error: Receiver email is incorrect: {receiverEmail}");
                    if (purchasedProduct == null)
                        _dbHelper.LogError($"IPN Error: Product not found or mismatched. ItemNumber: {itemNumber}");
                    if (purchasedProduct != null && purchasedProduct.ProductPrice != mcGross)
                        _dbHelper.LogError($"IPN Error: Price mismatch. Product Price: {purchasedProduct.ProductPrice}, PayPal Amount: {mcGross}");
                }
            }
            else if (verificationResponse.Equals("INVALID", StringComparison.OrdinalIgnoreCase))
            {
                _dbHelper.LogError($"IPN Invalid: Invalid verification response received. IPN Body: {requestBody}");
            }
            else
            {
                _dbHelper.LogError($"IPN Error: Unexpected verification response: {verificationResponse}. IPN Body: {requestBody}");
            }
        }

        private Dictionary<string, string> ParseIpnBody(string body)
        {
            var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var pairs = body.Split('&');
            foreach (var pair in pairs)
            {
                var parts = pair.Split('=');
                if (parts.Length == 2)
                {
                    data[HttpUtility.UrlDecode(parts[0])] = HttpUtility.UrlDecode(parts[1]);
                }
            }
            return data;
        }
    }
}