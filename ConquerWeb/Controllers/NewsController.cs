using Microsoft.AspNetCore.Mvc;
using ConquerWeb.Models;
using ConquerWeb.Services;
using System.Collections.Generic;

namespace ConquerWeb.Controllers
{
    public class NewsController : Controller
    {
        private readonly DatabaseHelper _dbHelper;

        public NewsController(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public IActionResult Index()
        {
            List<News> allNews = _dbHelper.GetAllNews();
            return View(allNews);
        }

        public IActionResult Details(int id)
        {
            News newsItem = _dbHelper.GetNewsById(id);
            if (newsItem == null)
            {
                return NotFound();
            }
            return View(newsItem);
        }
    }
}