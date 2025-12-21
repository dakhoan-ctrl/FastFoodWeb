using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FastFoodWeb.Models; // Đảm bảo đúng namespace của dự án bạn

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Database (Kết nối SQL Server)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 2. Cấu hình Identity (Đăng nhập/Đăng ký)
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>() // Thêm dòng này để hỗ trợ phân quyền Admin
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

// 3. CẤU HÌNH SESSION (Dùng cho Giỏ hàng) - QUAN TRỌNG
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Giỏ hàng tự xóa sau 30 phút vắng mặt
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// 4. Cấu hình Pipeline xử lý HTTP
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

// 5. KÍCH HOẠT SESSION - QUAN TRỌNG (Phải đặt sau UseRouting và trước UseAuthorization)
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// 6. Cấu hình Route (Đường dẫn mặc định)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();