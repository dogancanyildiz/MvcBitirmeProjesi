using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MvcBitirmeProjesi.Data;

var builder = WebApplication.CreateBuilder(args);

// SQLite bağlantısı
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// MVC, Session ve Authentication servisi eklendi
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache(); // Session için önbellek
builder.Services.AddSession();                // Session middleware

// Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";      // Giriş sayfası
        options.LogoutPath = "/Login/Logout";    // Çıkış sayfası
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Pipeline ayarları
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();       // Statik dosyalar (CSS, JS vs.)

app.UseRouting();
app.UseSession();           // 👈 Session burada olmalı
app.UseAuthentication();    // 👈 Login kontrolü
app.UseAuthorization();     // 👈 Yetki kontrolü

// Route ayarları
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();