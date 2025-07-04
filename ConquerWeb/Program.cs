using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims; // Although not directly used in Program.cs snippet, it's relevant for auth
using ConquerWeb.Models; // Assuming your models like Account, Character, etc. are here
using ConquerWeb.Services; // Assuming your services like DatabaseHelper, SecurityHelper, PayPalClient are here
using System; // For TimeSpan

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Bellek içi önbellekleme servisini ekle (IMemoryCache için)
builder.Services.AddMemoryCache();

// DatabaseHelper, SecurityHelper ve PayPalClient sýnýflarýný baðýmlýlýk enjeksiyonuna ekle
// DatabaseHelper ve SecurityHelper için Scoped ömrü çoðu senaryo için uygundur.
// PayPalClient için Singleton ömrü, uygulamanýn ömrü boyunca tek bir örnek kullanýlmasýný saðlar.
builder.Services.AddScoped<DatabaseHelper>();
builder.Services.AddScoped<SecurityHelper>();
builder.Services.AddSingleton<PayPalClient>();

// HttpClientFactory hizmetini ekle (PayPalClient veya diðer HTTP istekleri için gerekli)
builder.Services.AddHttpClient();

// Oturum hizmetini ekle
// Bu, HTTP istekleri arasýnda kullanýcý verilerini depolamak için kullanýlýr.
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Oturum süresi (örneðin 30 dakika)
    options.Cookie.HttpOnly = true; // Sadece HTTP üzerinden eriþilebilir çerezler (JavaScript eriþimini engeller)
    options.Cookie.IsEssential = true; // Oturum için gerekli çerezler (KVKK / GDPR uyumluluðu için önemli olabilir)
});

// Kimlik doðrulama hizmetini ekle (Cookie tabanlý kimlik doðrulama)
// Bu, uygulamanýzýn kullanýcýlarý nasýl tanýdýðýný ve oturumlarýný nasýl yönettiðini yapýlandýrýr.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login"; // Kullanýcý giriþ yapmadýðýnda yönlendirilecek yol
                                      // Not: Sizin eski kodunuzda "/Account/Login" idi. Eðer AccountController'ýnýz Login action'ýný bu yolda bekliyorsa bu doðru.
                                      // Routing'de Login'i Account/Login'e maplediðiniz için "/Login" sorun olmayacaktýr.
        options.LogoutPath = "/Account/Logout"; // Çýkýþ yapýldýðýnda yönlendirilecek yol
        options.AccessDeniedPath = "/Account/AccessDenied"; // Yetkisiz eriþimde yönlendirilecek yol
        options.ExpireTimeSpan = TimeSpan.FromDays(7); // Kimlik doðrulama çerezinin geçerlilik süresi (örneðin 7 gün)
        options.SlidingExpiration = true; // Çerez ömrü, kullanýcý etkinken uzatýlýr (yeniden oturum açmayý geciktirir)
    });

// Yetkilendirme hizmetini ekle
// Bu, kimlik doðrulandýktan sonra kullanýcýnýn belirli kaynaklara eriþim yetkisini kontrol eder.
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Geliþtirme ortamý deðilse hata iþleme ve HSTS'yi kullan.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Hata durumunda yönlendirilecek sayfa
    app.UseHsts(); // HTTP Strict Transport Security
}

// HTTP isteklerini HTTPS'ye yönlendir
app.UseHttpsRedirection();

// wwwroot klasöründeki statik dosyalarý (CSS, JS, resimler) sun
app.UseStaticFiles();

// Endpoint routing'i etkinleþtir
app.UseRouting();

// Oturum middleware'ini ekle
// Oturum verilerine eriþim saðlamak için UseRouting sonrasý, UseAuthentication öncesi olmalý.
app.UseSession();

// Kimlik doðrulama middleware'ini ekle
// Kimliði doðrulanmýþ kullanýcýnýn kimliðini HTTP baðlamýna ekler.
app.UseAuthentication();

// Yetkilendirme middleware'ini ekle
// Kimliði doðrulanmýþ kullanýcýnýn yetkilerini kontrol eder. UseAuthentication sonrasý olmalý.
app.UseAuthorization();

// Controller route'larýný ayarla
// PHP uyumluluðu veya özel URL yapýlandýrmalarý için özel rotalar
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


// Varsayýlan route her zaman en sonda olmalý
// Bu, uygulamanýzdaki genel Controller/Action/ID deseni için kullanýlýr.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();