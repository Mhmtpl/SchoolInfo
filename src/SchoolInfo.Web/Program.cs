using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SchoolInfo.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC Controller ve View yapılandırması
builder.Services.AddControllersWithViews();

// HttpContext ve Claims erişimi için
builder.Services.AddHttpContextAccessor();

// Cookie Kimlik Doğrulama Yapılandırması
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.Cookie.Name = "SchoolInfo.Session";
    });

// API Haberleşme Servisi (Typed HttpClient)
builder.Services.AddHttpClient<SchoolInfoApiService>();

var app = builder.Build();

// HTTP İstek Boru Hattı Yapılandırması
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Programatik olarak static asset'leri sunuyoruz

app.UseRouting();

// Kimlik Doğrulama Middleware Zinciri
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
