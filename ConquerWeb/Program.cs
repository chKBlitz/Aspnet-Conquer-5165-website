using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using ConquerWeb.Models;
using ConquerWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Bellek içi önbellekleme servisini ekle
builder.Services.AddMemoryCache();

// Oturum hizmetini ekle
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Oturum süresi
    options.Cookie.HttpOnly = true; // Sadece HTTP üzerinden eriþilebilir çerezler
    options.Cookie.IsEssential = true; // Oturum için gerekli çerezler
});

// DatabaseHelper, SecurityHelper ve PayPalClient sýnýflarýný baðýmlýlýk enjeksiyonuna ekle
builder.Services.AddScoped<DatabaseHelper>(); // Her istek için yeni bir örnek
builder.Services.AddScoped<SecurityHelper>(); // Her istek için yeni bir örnek
builder.Services.AddSingleton<PayPalClient>(); // Uygulama ömrü boyunca tek örnek

// HttpClientFactory hizmetini ekle (PaymentController için gerekli)
builder.Services.AddHttpClient();


// Kimlik doðrulama hizmetini ekle (Cookie tabanlý kimlik doðrulama)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login"; // Kullanýcý giriþ yapmadýðýnda yönlendirilecek yol
        options.LogoutPath = "/Account/Logout"; // Çýkýþ yapýldýðýnda yönlendirilecek yol
        options.AccessDeniedPath = "/Account/AccessDenied"; // Yetkisiz eriþimde yönlendirilecek yol
        options.ExpireTimeSpan = TimeSpan.FromDays(7); // Kimlik doðrulama çerezinin geçerlilik süresi
        options.SlidingExpiration = true; // Çerez ömrü, kullanýcý etkinken uzatýlýr
    });

builder.Services.AddAuthorization(); // Yetkilendirme hizmetini ekle

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Hata durumunda yönlendirilecek sayfa
    app.UseHsts();
}

app.UseHttpsRedirection(); // HTTP isteklerini HTTPS'ye yönlendir
app.UseStaticFiles(); // wwwroot klasöründeki statik dosyalarý (CSS, JS, resimler) sun

app.UseRouting(); // Endpoint routing'i etkinleþtir

app.UseSession(); // Oturumu kullan (UseRouting sonrasý, UseAuthentication öncesi olmalý)

app.UseAuthentication(); // Kimlik doðrulama middleware'ini ekle
app.UseAuthorization();  // Yetkilendirme middleware'ini ekle (UseAuthentication sonrasý olmalý)

// Controller route'larýný ayarla
app.MapControllerRoute(
    name: "login_php_compat",
    pattern: "Login", // PHP'deki /Login karþýlýðý
    defaults: new { controller = "Account", action = "Login" });

app.MapControllerRoute(
    name: "register_php_compat",
    pattern: "Register", // PHP'deki /Register karþýlýðý
    defaults: new { controller = "Account", action = "Register" });

app.MapControllerRoute(
    name: "home_php_compat",
    pattern: "home", // PHP'deki /home karþýlýðý
    defaults: new { controller = "Home", action = "Index" });

app.MapControllerRoute(
    name: "store_php_compat",
    pattern: "Store", // PHP'deki /Store karþýlýðý
    defaults: new { controller = "Store", action = "Index" });

app.MapControllerRoute(
    name: "success_php_compat",
    pattern: "success", // PHP'deki /success karþýlýðý (PayPal Return URL'si)
    defaults: new { controller = "Success", action = "Index" });

app.MapControllerRoute(
    name: "paymentshandler_ipn",
    pattern: "paymentshandler", // PayPal geliþtirici panelinde bu URL'yi IPN URL'si olarak ayarlayýn
    defaults: new { controller = "Payment", action = "Receive" });


app.MapControllerRoute(
    name: "default", // Varsayýlan route her zaman en sonda olmalý
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();