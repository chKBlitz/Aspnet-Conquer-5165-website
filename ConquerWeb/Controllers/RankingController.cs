using Microsoft.AspNetCore.Mvc;
using ConquerWeb.Models;
using ConquerWeb.Services;
using System.Collections.Generic;

namespace ConquerWeb.Controllers
{
    public class RankingController : Controller
    {
        private readonly DatabaseHelper _dbHelper;

        public RankingController(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        [HttpGet]
        public IActionResult Index()
        {
            List<TopPlayer> topPlayers = _dbHelper.GetTopPlayersFromTopsTable(topType: 1, count: 30);
            ViewData["Title"] = "Player Rankings";
            return View(topPlayers);
        }

        [HttpGet]
        public IActionResult Guild()
        {
            List<Guild> topGuilds = _dbHelper.GetTopGuilds(10);
            ViewData["Title"] = "Guild Rankings";
            return View(topGuilds);
        }
    }
}