using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using ConquerWeb.Services;
using ConquerWeb.Models;
using System.Security.Claims;
using System.Diagnostics;
using System.Collections.Generic;

namespace ConquerWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly DatabaseHelper _dbHelper;

        public HomeController(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                ViewBag.IsLoggedIn = true;
                ViewBag.Username = User.Identity.Name;

                int userId;
                if (int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out userId))
                {
                    Character userCharacter = _dbHelper.GetCharacterByUserId(userId);
                    ViewBag.UserCharacter = userCharacter;
                }
            }
            else
            {
                ViewBag.IsLoggedIn = false;
            }

            List<News> latestNews = _dbHelper.GetLatestNews(3);
            ViewBag.LatestNews = latestNews;

            OnlineStats onlineStats = _dbHelper.GetOnlinePlayerStats();
            ViewBag.OnlineStats = onlineStats;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult RefundPolicy()
        {
            ViewData["Title"] = "Refund Policy";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}