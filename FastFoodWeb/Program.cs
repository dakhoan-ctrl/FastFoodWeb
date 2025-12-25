using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FastFoodWeb.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 2. Cấu hình Identity
builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login"; // Chuyển hướng đến Controller của bạn
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// --- THÊM ĐOẠN NÀY: CẤU HÌNH GOOGLE VÀ FACEBOOK ---
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        // Bạn lấy mã này từ https://console.cloud.google.com/
        options.ClientId = "MÃ_CLIENT_ID_GOOGLE_CUA_BAN";
        options.ClientSecret = "MÃ_SECRET_GOOGLE_CUA_BAN";
    })
    .AddFacebook(options =>
    {
        // Bạn lấy mã này từ https://developers.facebook.com/
        options.AppId = "MÃ_APP_ID_FACEBOOK_CUA_BAN";
        options.AppSecret = "MÃ_APP_SECRET_FACEBOOK_CUA_BAN";
    });
// ----------------------------------------------------

builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();

// 3. Cấu hình Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// 4. Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();