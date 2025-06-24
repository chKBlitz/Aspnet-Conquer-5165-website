using Microsoft.AspNetCore.Mvc;
using ConquerWeb.Services;
using PayPal.Api;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System;
using PayPal.Exception;

namespace ConquerWeb.Controllers
{
    [Authorize]
    public class SuccessController : Controller
    {
        private readonly PayPalClient _payPalClient;
        private readonly DatabaseHelper _dbHelper;

        public SuccessController(PayPalClient payPalClient, DatabaseHelper dbHelper)
        {
            _payPalClient = payPalClient;
            _dbHelper = dbHelper;
        }

        public IActionResult Index(string paymentId, string PayerID)
        {
            if (string.IsNullOrEmpty(paymentId) || string.IsNullOrEmpty(PayerID))
            {
                _dbHelper.LogError("PayPal Success: Missing paymentId or PayerID.");
                ViewBag.ErrorMessage = "Payment details are missing or invalid. Please check your PayPal transaction history.";
                return View("Error");
            }

            try
            {
                Payment payment = _payPalClient.ExecutePayment(paymentId, PayerID);

                if (payment.state.Equals("approved", StringComparison.OrdinalIgnoreCase))
                {
                    return View(payment);
                }
                else
                {
                    _dbHelper.LogError($"PayPal Success: Unapproved payment status: {payment.state}. Payment ID: {paymentId}");
                    ViewBag.ErrorMessage = $"Payment status not approved: {payment.state}. Please check your PayPal transaction history.";
                    return View("Error");
                }
            }
            catch (PayPalException ex)
            {
                _dbHelper.LogError($"PayPal API Error (ExecutePayment): {ex.Message} - Details: {ex.Data}");
                ViewBag.ErrorMessage = $"An error occurred while completing the payment: {ex.Message}. Please try again or check your PayPal transaction history.";
                return View("Error");
            }
            catch (Exception ex)
            {
                _dbHelper.LogError($"Unexpected error (SuccessController): {ex.Message} - Payment ID: {paymentId}");
                ViewBag.ErrorMessage = "An unexpected error occurred. Please try again.";
                return View("Error");
            }
        }
    }
}