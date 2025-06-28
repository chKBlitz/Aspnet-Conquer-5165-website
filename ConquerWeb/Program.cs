using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using ConquerWeb.Models;
using ConquerWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Bellek i�i �nbellekleme servisini ekle
builder.Services.AddMemoryCache();

// Oturum hizmetini ekle
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Oturum s�resi
    options.Cookie.HttpOnly = true; // Sadece HTTP �zerinden eri�ilebilir �erezler
    options.Cookie.IsEssential = true; // Oturum i�in gerekli �erezler
});

// DatabaseHelper, SecurityHelper ve PayPalClient s�n�flar�n� ba��ml�l�k enjeksiyonuna ekle
builder.Services.AddScoped<DatabaseHelper>(); // Her istek i�in yeni bir �rnek
builder.Services.AddScoped<SecurityHelper>(); // Her istek i�in yeni bir �rnek
builder.Services.AddSingleton<PayPalClient>(); // Uygulama �mr� boyunca tek �rnek

// HttpClientFactory hizmetini ekle (PaymentController i�in gerekli)
builder.Services.AddHttpClient();


// Kimlik do�rulama hizmetini ekle (Cookie tabanl� kimlik do�rulama)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login"; // Kullan�c� giri� yapmad���nda y�nlendirilecek yol
        options.LogoutPath = "/Account/Logout"; // ��k�� yap�ld���nda y�nlendirilecek yol
        options.AccessDeniedPath = "/Account/AccessDenied"; // Yetkisiz eri�imde y�nlendirilecek yol
        options.ExpireTimeSpan = TimeSpan.FromDays(7); // Kimlik do�rulama �erezinin ge�erlilik s�resi
        options.SlidingExpiration = true; // �erez �mr�, kullan�c� etkinken uzat�l�r
    });

builder.Services.AddAuthorization(); // Yetkilendirme hizmetini ekle

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Hata durumunda y�nlendirilecek sayfa
    app.UseHsts();
}

app.UseHttpsRedirection(); // HTTP isteklerini HTTPS'ye y�nlendir
app.UseStaticFiles(); // wwwroot klas�r�ndeki statik dosyalar� (CSS, JS, resimler) sun

app.UseRouting(); // Endpoint routing'i etkinle�tir

app.UseSession(); // Oturumu kullan (UseRouting sonras�, UseAuthentication �ncesi olmal�)

app.UseAuthentication(); // Kimlik do�rulama middleware'ini ekle
app.UseAuthorization();  // Yetkilendirme middleware'ini ekle (UseAuthentication sonras� olmal�)

// Controller route'lar�n� ayarla
app.MapControllerRoute(
    name: "login_php_compat",
    pattern: "Login", // PHP'deki /Login kar��l���
    defaults: new { controller = "Account", action = "Login" });

app.MapControllerRoute(
    name: "register_php_compat",
    pattern: "Register", // PHP'deki /Register kar��l���
    defaults: new { controller = "Account", action = "Register" });

app.MapControllerRoute(
    name: "home_php_compat",
    pattern: "home", // PHP'deki /home kar��l���
    defaults: new { controller = "Home", action = "Index" });

app.MapControllerRoute(
    name: "store_php_compat",
    pattern: "Store", // PHP'deki /Store kar��l���
    defaults: new { controller = "Store", action = "Index" });

app.MapControllerRoute(
    name: "success_php_compat",
    pattern: "success", // PHP'deki /success kar��l��� (PayPal Return URL'si)
    defaults: new { controller = "Success", action = "Index" });

app.MapControllerRoute(
    name: "paymentshandler_ipn",
    pattern: "paymentshandler", // PayPal geli�tirici panelinde bu URL'yi IPN URL'si olarak ayarlay�n
    defaults: new { controller = "Payment", action = "Receive" });


app.MapControllerRoute(
    name: "default", // Varsay�lan route her zaman en sonda olmal�
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();