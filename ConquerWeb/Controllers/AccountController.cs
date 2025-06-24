using ConquerWeb.Models;
using ConquerWeb.Models.ViewModels;
using ConquerWeb.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ConquerWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly SecurityHelper _securityHelper;

        public AccountController(DatabaseHelper dbHelper, SecurityHelper securityHelper)
        {
            _dbHelper = dbHelper;
            _securityHelper = securityHelper;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_dbHelper.GetAccountByUsername(model.Username) != null)
                {
                    ModelState.AddModelError("Username", "This username is already in use.");
                    return View(model);
                }

                if (_dbHelper.GetAccountByEmail(model.Email) != null)
                {
                    ModelState.AddModelError("Email", "This email address is already in use.");
                    return View(model);
                }

                string plainPassword = _securityHelper.HashPassword(model.Password);
                string userIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                bool success = _dbHelper.RegisterUser(model.Username, plainPassword, model.Email, userIpAddress);

                if (success)
                {
                    var userAccount = _dbHelper.GetAccountByUsername(model.Username);
                    if (userAccount != null)
                    {
                        await SignInUser(userAccount, false);
                        _dbHelper.UpdateLastLogin(userAccount.UID, userIpAddress);
                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError("", "An error occurred during registration.");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var account = _dbHelper.GetAccountByUsername(model.Username);

                if (account != null && _securityHelper.VerifyPassword(model.Password, account.Password))
                {
                    string userIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                    await SignInUser(account, model.RememberMe);
                    _dbHelper.UpdateLastLogin(account.UID, userIpAddress);
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Invalid username or password.");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        private async Task SignInUser(Account account, bool isPersistent)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, account.UID.ToString()),
                new Claim(ClaimTypes.Name, account.Username),
                new Claim(ClaimTypes.Email, account.Email)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                ExpiresUtc = isPersistent ? DateTimeOffset.UtcNow.AddDays(7) : (DateTimeOffset?)null
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            HttpContext.Session.SetInt32("UserID", account.UID);
            HttpContext.Session.SetString("Username", account.Username);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var account = _dbHelper.GetAccountByUsername(User.Identity.Name);

                if (account == null || !_securityHelper.VerifyPassword(model.OldPassword, account.Password))
                {
                    ModelState.AddModelError("OldPassword", "Current password is incorrect.");
                    return View(model);
                }

                _dbHelper.UpdatePassword(userId, _securityHelper.HashPassword(model.NewPassword));

                ViewBag.SuccessMessage = "Your password has been changed successfully.";
                ModelState.Clear();
                return View();
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var account = _dbHelper.GetAccountByEmail(model.Email);

                if (account != null)
                {
                    string token = _securityHelper.GenerateResetToken();
                    DateTime expiry = DateTime.UtcNow.AddHours(24);

                    _dbHelper.SetResetToken(account.UID, token, expiry);

                    string resetLink = Url.Action("ResetPassword", "Account", new { token = token, email = model.Email }, Request.Scheme);
                    _dbHelper.LogError($"Password reset token generated: Email: {account.Email}, Token: {token}, Link: {resetLink}");

                    ViewBag.Message = "A password reset link has been sent to your email address.";
                    ModelState.Clear();
                }
                else
                {
                    ViewBag.Message = "A password reset link has been sent to your email address.";
                    ModelState.Clear();
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                ViewBag.ErrorMessage = "Invalid password reset link.";
                return View("Error");
            }

            var account = _dbHelper.GetAccountByResetToken(token);

            if (account == null || account.Email != email)
            {
                ViewBag.ErrorMessage = "Invalid or expired password reset link.";
                return View("Error");
            }

            var model = new ResetPasswordViewModel { Token = token, Email = email };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var account = _dbHelper.GetAccountByResetToken(model.Token);

                if (account == null || account.Email != model.Email)
                {
                    ModelState.AddModelError("", "Invalid or expired password reset link.");
                    return View(model);
                }

                _dbHelper.UpdatePassword(account.UID, _securityHelper.HashPassword(model.NewPassword));
                _dbHelper.ClearResetToken(account.UID);

                if (User.Identity.IsAuthenticated && User.FindFirst(ClaimTypes.NameIdentifier)?.Value == account.UID.ToString())
                {
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }

                ViewBag.SuccessMessage = "Your password has been reset successfully. Please log in with your new password.";
                ModelState.Clear();
                return RedirectToAction("Login", "Account");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public IActionResult Profile()
        {
            int userId;
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out userId))
            {
                _dbHelper.LogError($"Profile Error: User UID could not be retrieved from Claims. Username: {User.Identity.Name}");
                return RedirectToAction("Login", "Account");
            }

            var account = _dbHelper.GetAccountByUsername(User.Identity.Name);
            if (account == null || account.UID != userId)
            {
                _dbHelper.LogError($"Profile Error: Account information not found or mismatched. UID: {userId}");
                return RedirectToAction("Login", "Account");
            }

            var character = _dbHelper.GetCharacterByUserId(userId);

            var viewModel = new UserProfileViewModel
            {
                Account = account,
                Character = character
            };

            return View(viewModel);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult IsUsernameAvailable(string username)
        {
            if (string.IsNullOrWhiteSpace(username) || username.Length < 4)
            {
                return Json(false);
            }

            var account = _dbHelper.GetAccountByUsername(username);
            return Json(account == null);
        }
    }
}