using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims; // Although not directly used in Program.cs snippet, it's relevant for auth
using ConquerWeb.Models; // Assuming your models like Account, Character, etc. are here
using ConquerWeb.Services; // Assuming your services like DatabaseHelper, SecurityHelper, PayPalClient are here
using System; // For TimeSpan

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Bellek i�i �nbellekleme servisini ekle (IMemoryCache i�in)
builder.Services.AddMemoryCache();

// DatabaseHelper, SecurityHelper ve PayPalClient s�n�flar�n� ba��ml�l�k enjeksiyonuna ekle
// DatabaseHelper ve SecurityHelper i�in Scoped �mr� �o�u senaryo i�in uygundur.
// PayPalClient i�in Singleton �mr�, uygulaman�n �mr� boyunca tek bir �rnek kullan�lmas�n� sa�lar.
builder.Services.AddScoped<DatabaseHelper>();
builder.Services.AddScoped<SecurityHelper>();
builder.Services.AddSingleton<PayPalClient>();

// HttpClientFactory hizmetini ekle (PayPalClient veya di�er HTTP istekleri i�in gerekli)
builder.Services.AddHttpClient();

// Oturum hizmetini ekle
// Bu, HTTP istekleri aras�nda kullan�c� verilerini depolamak i�in kullan�l�r.
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Oturum s�resi (�rne�in 30 dakika)
    options.Cookie.HttpOnly = true; // Sadece HTTP �zerinden eri�ilebilir �erezler (JavaScript eri�imini engeller)
    options.Cookie.IsEssential = true; // Oturum i�in gerekli �erezler (KVKK / GDPR uyumlulu�u i�in �nemli olabilir)
});

// Kimlik do�rulama hizmetini ekle (Cookie tabanl� kimlik do�rulama)
// Bu, uygulaman�z�n kullan�c�lar� nas�l tan�d���n� ve oturumlar�n� nas�l y�netti�ini yap�land�r�r.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login"; // Kullan�c� giri� yapmad���nda y�nlendirilecek yol
                                      // Not: Sizin eski kodunuzda "/Account/Login" idi. E�er AccountController'�n�z Login action'�n� bu yolda bekliyorsa bu do�ru.
                                      // Routing'de Login'i Account/Login'e mapledi�iniz i�in "/Login" sorun olmayacakt�r.
        options.LogoutPath = "/Account/Logout"; // ��k�� yap�ld���nda y�nlendirilecek yol
        options.AccessDeniedPath = "/Account/AccessDenied"; // Yetkisiz eri�imde y�nlendirilecek yol
        options.ExpireTimeSpan = TimeSpan.FromDays(7); // Kimlik do�rulama �erezinin ge�erlilik s�resi (�rne�in 7 g�n)
        options.SlidingExpiration = true; // �erez �mr�, kullan�c� etkinken uzat�l�r (yeniden oturum a�may� geciktirir)
    });

// Yetkilendirme hizmetini ekle
// Bu, kimlik do�ruland�ktan sonra kullan�c�n�n belirli kaynaklara eri�im yetkisini kontrol eder.
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Geli�tirme ortam� de�ilse hata i�leme ve HSTS'yi kullan.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Hata durumunda y�nlendirilecek sayfa
    app.UseHsts(); // HTTP Strict Transport Security
}

// HTTP isteklerini HTTPS'ye y�nlendir
app.UseHttpsRedirection();

// wwwroot klas�r�ndeki statik dosyalar� (CSS, JS, resimler) sun
app.UseStaticFiles();

// Endpoint routing'i etkinle�tir
app.UseRouting();

// Oturum middleware'ini ekle
// Oturum verilerine eri�im sa�lamak i�in UseRouting sonras�, UseAuthentication �ncesi olmal�.
app.UseSession();

// Kimlik do�rulama middleware'ini ekle
// Kimli�i do�rulanm�� kullan�c�n�n kimli�ini HTTP ba�lam�na ekler.
app.UseAuthentication();

// Yetkilendirme middleware'ini ekle
// Kimli�i do�rulanm�� kullan�c�n�n yetkilerini kontrol eder. UseAuthentication sonras� olmal�.
app.UseAuthorization();

// Controller route'lar�n� ayarla
// PHP uyumlulu�u veya �zel URL yap�land�rmalar� i�in �zel rotalar
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


// Varsay�lan route her zaman en sonda olmal�
// Bu, uygulaman�zdaki genel Controller/Action/ID deseni i�in kullan�l�r.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();