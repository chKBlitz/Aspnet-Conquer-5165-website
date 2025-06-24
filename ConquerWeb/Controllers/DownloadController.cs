using Microsoft.AspNetCore.Mvc;
using ConquerWeb.Models;
using ConquerWeb.Services;
using System.Collections.Generic;

namespace ConquerWeb.Controllers
{
    public class DownloadController : Controller
    {
        private readonly DatabaseHelper _dbHelper;

        public DownloadController(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public IActionResult Index()
        {
            List<DownloadItem> downloadItems = _dbHelper.GetAllDownloads();
            return View(downloadItems);
        }
    }
}