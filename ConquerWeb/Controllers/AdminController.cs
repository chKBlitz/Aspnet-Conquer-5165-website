using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ConquerWeb.Services;
using ConquerWeb.Models;
using ConquerWeb.Models.ViewModels; // EditUserViewModel, EditNewsViewModel, EditProductViewModel, AdminDashboardViewModel
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;
using System; // DateTime için

namespace ConquerWeb.Controllers
{
    // Sadece "Admin" rolüne sahip kullanıcıların erişimine izin ver
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly DatabaseHelper _dbHelper;

        public AdminController(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // GET: /Admin/Index (Admin Paneli Ana Sayfası)
        public IActionResult Index()
        {
            var onlineStats = _dbHelper.GetOnlinePlayerStats();
            var totalUsers = _dbHelper.GetTotalUserCount();

            var viewModel = new AdminDashboardViewModel
            {
                OnlinePlayers = onlineStats?.Online ?? 0,
                MaxPlayers = onlineStats?.Max ?? 0,
                TotalUsers = totalUsers
            };

            ViewData["Title"] = "Admin Panel";
            return View(viewModel);
        }

        // GET: /Admin/Users (Tüm kullanıcıları listeleme)
        [HttpGet]
        public IActionResult Users()
        {
            List<Account> allAccounts = _dbHelper.GetAllAccounts();
            ViewData["Title"] = "Manage Users";
            return View(allAccounts);
        }

        // GET: /Admin/EditUser/{uid} (Kullanıcı düzenleme formunu gösterme)
        [HttpGet]
        public IActionResult EditUser(int uid)
        {
            var userToEdit = _dbHelper.GetAccountByUid(uid);

            if (userToEdit == null)
            {
                TempData["SweetAlertError"] = "Düzenlenecek kullanıcı bulunamadı.";
                return RedirectToAction("Users");
            }

            var viewModel = new EditUserViewModel
            {
                UID = userToEdit.UID,
                Username = userToEdit.Username,
                Email = userToEdit.Email,
                Status = userToEdit.Status
            };

            ViewData["Title"] = $"Kullanıcı Düzenle: {userToEdit.Username}";
            return View(viewModel);
        }

        // POST: /Admin/EditUser (Kullanıcı bilgilerini güncelleme)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUser(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingAccountByUsername = _dbHelper.GetAccountByUsername(model.Username);
                if (existingAccountByUsername != null && existingAccountByUsername.UID != model.UID)
                {
                    ModelState.AddModelError("Username", "Bu kullanıcı adı zaten kullanımda.");
                    TempData["SweetAlertError"] = "Bu kullanıcı adı zaten kullanımda.";
                    ViewData["Title"] = $"Kullanıcı Düzenle: {model.Username}";
                    return View(model);
                }

                var existingAccountByEmail = _dbHelper.GetAccountByEmail(model.Email);
                if (existingAccountByEmail != null && existingAccountByEmail.UID != model.UID)
                {
                    ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanımda.");
                    TempData["SweetAlertError"] = "Bu e-posta adresi zaten kullanımda.";
                    ViewData["Title"] = $"Kullanıcı Düzenle: {model.Username}";
                    return View(model);
                }

                bool success = _dbHelper.UpdateAccount(model.UID, model.Username, model.Email, model.Status);

                if (success)
                {
                    TempData["SweetAlertSuccess"] = $"Kullanıcı '{model.Username}' başarıyla güncellendi!";
                    return RedirectToAction("Users");
                }
                else
                {
                    ModelState.AddModelError("", "Kullanıcı güncellenirken bir hata oluştu.");
                    TempData["SweetAlertError"] = $"Kullanıcı '{model.Username}' güncellenirken bir hata oluştu.";
                    ViewData["Title"] = $"Kullanıcı Düzenle: {model.Username}";
                    return View(model);
                }
            }
            ViewData["Title"] = $"Kullanıcı Düzenle: {model.Username}";
            return View(model);
        }

        // POST: /Admin/DeleteUser (Kullanıcıyı silme)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUser(int uid)
        {
            int currentAdminUid;
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out currentAdminUid))
            {
                TempData["SweetAlertError"] = "Geçerli admin kimliği alınamadı.";
                return RedirectToAction("Users");
            }

            if (uid == currentAdminUid)
            {
                TempData["SweetAlertError"] = "Kendi yöneticiliğinizi silemezsiniz!";
                return RedirectToAction("Users");
            }

            bool success = _dbHelper.DeleteAccount(uid);

            if (success)
            {
                TempData["SweetAlertSuccess"] = "Kullanıcı başarıyla silindi.";
            }
            else
            {
                TempData["SweetAlertError"] = "Kullanıcı silinirken bir hata oluştu.";
            }
            return RedirectToAction("Users");
        }

        // GET: /Admin/ViewLogs (Uygulama loglarını görüntüleme)
        [HttpGet]
        public IActionResult ViewLogs()
        {
            List<LogEntry> logs = _dbHelper.GetLogs();
            ViewData["Title"] = "Uygulama Logları";
            return View(logs);
        }

        // --- HABER YÖNETİMİ ACTION'LARI ---

        // GET: /Admin/News (Tüm haberleri listeleme)
        [HttpGet]
        public IActionResult News()
        {
            List<News> allNews = _dbHelper.GetAllNews();
            ViewData["Title"] = "Manage News";
            return View(allNews);
        }

        // GET: /Admin/AddNews (Yeni haber ekleme formu gösterme)
        [HttpGet]
        public IActionResult AddNews()
        {
            ViewData["Title"] = "Add New News";
            return View(new EditNewsViewModel { Publish_Date = DateTime.Now, Author = User.Identity.Name });
        }

        // POST: /Admin/AddNews (Yeni haber kaydetme)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddNews(EditNewsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var newsToAdd = new News
                {
                    Title = model.Title,
                    Content = model.Content,
                    Author = model.Author,
                    Publish_Date = model.Publish_Date
                };

                bool success = _dbHelper.AddNews(newsToAdd);

                if (success)
                {
                    TempData["SweetAlertSuccess"] = "Haber başarıyla eklendi!";
                    return RedirectToAction("News");
                }
                else
                {
                    ModelState.AddModelError("", "Haber eklenirken bir hata oluştu.");
                    TempData["SweetAlertError"] = "Haber eklenirken bir hata oluştu.";
                }
            }
            ViewData["Title"] = "Add New News";
            return View(model);
        }

        // GET: /Admin/EditNews/{id} (Haber düzenleme formu gösterme)
        [HttpGet]
        public IActionResult EditNews(int id)
        {
            var newsToEdit = _dbHelper.GetNewsById(id);

            if (newsToEdit == null)
            {
                TempData["SweetAlertError"] = "Düzenlenecek haber bulunamadı.";
                return RedirectToAction("News");
            }

            var viewModel = new EditNewsViewModel
            {
                Id = newsToEdit.Id,
                Title = newsToEdit.Title,
                Content = newsToEdit.Content,
                Author = newsToEdit.Author,
                Publish_Date = newsToEdit.Publish_Date
            };

            ViewData["Title"] = $"Haber Düzenle: {newsToEdit.Title}";
            return View(viewModel);
        }

        // POST: /Admin/EditNews (Haber bilgilerini güncelleme)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditNews(EditNewsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var newsToUpdate = new News
                {
                    Id = model.Id,
                    Title = model.Title,
                    Content = model.Content,
                    Author = model.Author,
                    Publish_Date = model.Publish_Date
                };

                bool success = _dbHelper.UpdateNews(newsToUpdate);

                if (success)
                {
                    TempData["SweetAlertSuccess"] = $"Haber '{model.Title}' başarıyla güncellendi!";
                    return RedirectToAction("News");
                }
                else
                {
                    ModelState.AddModelError("", "Haber güncellenirken bir hata oluştu.");
                    TempData["SweetAlertError"] = $"Haber '{model.Title}' güncellenirken bir hata oluştu.";
                }
            }
            ViewData["Title"] = $"Haber Düzenle: {model.Title}";
            return View(model);
        }

        // POST: /Admin/DeleteNews (Haber silme)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteNews(int id)
        {
            bool success = _dbHelper.DeleteNews(id);

            if (success)
            {
                TempData["SweetAlertSuccess"] = "Haber başarıyla silindi.";
            }
            else
            {
                TempData["SweetAlertError"] = "Haber silinirken bir hata oluştu.";
            }
            return RedirectToAction("News");
        }

        // --- YENİ EKLENEN ÜRÜN YÖNETİMİ ACTION'LARI ---

        // GET: /Admin/Products (Tüm ürünleri listeleme)
        [HttpGet]
        public IActionResult Products()
        {
            List<Product> allProducts = _dbHelper.GetAllProducts();
            ViewData["Title"] = "Manage Products";
            return View(allProducts);
        }

        // GET: /Admin/AddProduct (Yeni ürün ekleme formu gösterme)
        [HttpGet]
        public IActionResult AddProduct()
        {
            ViewData["Title"] = "Add New Product";
            return View(new EditProductViewModel()); // Boş bir ViewModel ile formu başlatıyoruz
        }

        // POST: /Admin/AddProduct (Yeni ürün kaydetme)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddProduct(EditProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                var productToAdd = new Product
                {
                    ProductName = model.ProductName,
                    ProductPrice = model.ProductPrice,
                    ProductCurrency = model.ProductCurrency,
                    ProductDesc = model.ProductDesc,
                    DBScrolls = model.DBScrolls,
                    // BURADA: ProductImage'e ön ek ekliyoruz
                    ProductImage = "/images/store/" + model.ProductImage // Otomatik yol ekleme
                };

                bool success = _dbHelper.AddProduct(productToAdd);

                if (success)
                {
                    TempData["SweetAlertSuccess"] = "Ürün başarıyla eklendi!";
                    return RedirectToAction("Products");
                }
                else
                {
                    ModelState.AddModelError("", "Ürün eklenirken bir hata oluştu.");
                    TempData["SweetAlertError"] = "Ürün eklenirken bir hata oluştu.";
                }
            }
            ViewData["Title"] = "Add New Product";
            return View(model);
        }

        // GET: /Admin/EditProduct/{productId} (Ürün düzenleme formu gösterme)
        [HttpGet]
        public IActionResult EditProduct(int productId)
        {
            var productToEdit = _dbHelper.GetProductById(productId);

            if (productToEdit == null)
            {
                TempData["SweetAlertError"] = "Düzenlenecek ürün bulunamadı.";
                return RedirectToAction("Products");
            }

            var viewModel = new EditProductViewModel
            {
                ProductId = productToEdit.ProductId,
                ProductName = productToEdit.ProductName,
                ProductPrice = productToEdit.ProductPrice,
                ProductCurrency = productToEdit.ProductCurrency,
                ProductDesc = productToEdit.ProductDesc,
                DBScrolls = productToEdit.DBScrolls,
                ProductImage = productToEdit.ProductImage
            };

            ViewData["Title"] = $"Ürün Düzenle: {productToEdit.ProductName}";
            return View(viewModel);
        }

        // POST: /Admin/EditProduct (Ürün bilgilerini güncelleme)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProduct(EditProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                var productToUpdate = new Product
                {
                    ProductId = model.ProductId,
                    ProductName = model.ProductName,
                    ProductPrice = model.ProductPrice,
                    ProductCurrency = model.ProductCurrency,
                    ProductDesc = model.ProductDesc,
                    DBScrolls = model.DBScrolls,
                    // BURADA: ProductImage'e ön ek ekliyoruz
                    ProductImage = "/images/store/" + model.ProductImage // Otomatik yol ekleme
                };

                bool success = _dbHelper.UpdateProduct(productToUpdate);

                if (success)
                {
                    TempData["SweetAlertSuccess"] = $"Ürün '{model.ProductName}' başarıyla güncellendi!";
                    return RedirectToAction("Products");
                }
                else
                {
                    ModelState.AddModelError("", "Ürün güncellenirken bir hata oluştu.");
                    TempData["SweetAlertError"] = $"Ürün '{model.ProductName}' güncellenirken bir hata oluştu.";
                }
            }
            ViewData["Title"] = $"Ürün Düzenle: {model.ProductName}";
            return View(model);
        }

        // POST: /Admin/DeleteProduct (Ürün silme)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteProduct(int productId)
        {
            bool success = _dbHelper.DeleteProduct(productId);

            if (success)
            {
                TempData["SweetAlertSuccess"] = "Ürün başarıyla silindi.";
            }
            else
            {
                TempData["SweetAlertError"] = "Ürün silinirken bir hata oluştu.";
            }
            return RedirectToAction("Products");
        }
    }
}