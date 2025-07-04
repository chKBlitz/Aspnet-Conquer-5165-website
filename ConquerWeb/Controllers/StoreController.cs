using Microsoft.AspNetCore.Mvc;
using ConquerWeb.Models;
using ConquerWeb.Services;
using Microsoft.AspNetCore.Http;
using System.Linq;
using PayPal.Api;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System;
using PayPal.Exception;

namespace ConquerWeb.Controllers
{
    [Authorize]
    public class StoreController : Controller
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly PayPalClient _payPalClient;

        public StoreController(DatabaseHelper dbHelper, PayPalClient payPalClient)
        {
            _dbHelper = dbHelper;
            _payPalClient = payPalClient;
        }

        public IActionResult Index()
        {
            int userId;
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out userId))
            {
                return RedirectToAction("Login", "Account");
            }

            Character character = _dbHelper.GetCharacterByUserId(userId);

            if (character == null)
            {
                ViewBag.ErrorMessage = "You must create a character before making a donation!";
                return View("NoCharacter"); // Bu View'in (Views/Store/NoCharacter.cshtml) mevcut olduğundan emin olun.
            }

            ViewBag.CharacterName = character.Name;
            ViewBag.DBScrolls = character.DBScrolls;
            var products = _dbHelper.GetAllProducts();
            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProcessPayment(int productId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            Character character = _dbHelper.GetCharacterByUserId(userId);

            if (character == null)
            {
                ViewBag.ErrorMessage = "Your character was not found. Please create a character first.";
                return RedirectToAction("Index"); // Mağaza ana sayfasına geri dön
            }

            Product product = _dbHelper.GetProductById(productId);

            if (product == null)
            {
                return NotFound("Product not found.");
            }

            try
            {
                Payment payment = _payPalClient.CreatePayment(product, character.Name);

                var approvalUrl = payment.links.FirstOrDefault(x => x.rel.Equals("approval_url", StringComparison.OrdinalIgnoreCase));
                if (approvalUrl != null)
                {
                    return Redirect(approvalUrl.href);
                }
                else
                {
                    _dbHelper.LogError($"PayPal approval URL not found. Payment ID: {payment.id}");
                    ViewBag.ErrorMessage = "Failed to create PayPal payment link. Please try again.";
                    return View("Error"); // Genişletilmiş hata sayfası
                }
            }
            catch (PayPalException ex)
            {
                _dbHelper.LogError($"PayPal API Error (CreatePayment): {ex.Message} - Details: {ex.Data}");
                ViewBag.ErrorMessage = $"An error occurred during PayPal payment: {ex.Message}";
                return View("Error"); // Genişletilmiş hata sayfası
            }
            catch (Exception ex)
            {
                _dbHelper.LogError($"Unexpected error (ProcessPayment): {ex.Message}");
                ViewBag.ErrorMessage = "An unexpected error occurred. Please try again.";
                return View("Error"); // Genişletilmiş hata sayfası
            }
        }
    }
}