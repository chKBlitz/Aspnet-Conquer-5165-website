using PayPal.Api;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Globalization;
using ConquerWeb.Models;
using PayPal.Exception; // Added for PayPalException

namespace ConquerWeb.Services
{
    public class PayPalClient
    {
        private readonly APIContext _apiContext;
        private readonly string _returnUrl;
        private readonly string _cancelUrl;
        private readonly string _mode;

        public PayPalClient(IConfiguration configuration)
        {
            var paypalSettings = configuration.GetSection("PayPalSettings");

            _returnUrl = paypalSettings["ReturnUrl"];
            _cancelUrl = paypalSettings["CancelUrl"];
            _mode = paypalSettings["Mode"];

            var config = new Dictionary<string, string>
            {
                { "mode", _mode }
            };

            var credential = new OAuthTokenCredential(paypalSettings["ClientId"], paypalSettings["ClientSecret"], config);
            string accessToken = credential.GetAccessToken();
            _apiContext = new APIContext(accessToken);
        }

        public Payment CreatePayment(Product product, string playerName)
        {
            var payer = new Payer { payment_method = "paypal" };

            var item = new Item
            {
                name = playerName,
                currency = product.ProductCurrency,
                quantity = "1",
                price = product.ProductPrice.ToString("F2", CultureInfo.InvariantCulture),
                sku = product.DBScrolls.ToString()
            };

            var itemList = new ItemList { items = new List<Item> { item } };

            var details = new Details
            {
                shipping = "0.00",
                tax = "0.00",
                subtotal = product.ProductPrice.ToString("F2", CultureInfo.InvariantCulture)
            };

            var amount = new Amount
            {
                currency = product.ProductCurrency,
                total = product.ProductPrice.ToString("F2", CultureInfo.InvariantCulture),
                details = details
            };

            var transaction = new Transaction
            {
                amount = amount,
                item_list = itemList,
                description = product.ProductName,
                invoice_number = System.Guid.NewGuid().ToString()
            };

            var redirectUrls = new RedirectUrls
            {
                return_url = _returnUrl,
                cancel_url = _cancelUrl
            };

            var payment = new Payment
            {
                intent = "sale",
                payer = payer,
                transactions = new List<Transaction> { transaction },
                redirect_urls = redirectUrls
            };

            return payment.Create(_apiContext);
        }

        public Payment ExecutePayment(string paymentId, string payerId)
        {
            var paymentExecution = new PaymentExecution { payer_id = payerId };
            var payment = new Payment { id = paymentId };
            return payment.Execute(_apiContext, paymentExecution);
        }
    }
}