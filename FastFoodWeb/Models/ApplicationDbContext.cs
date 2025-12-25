using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FastFoodWeb.Models; // Đảm bảo đúng namespace của dự án bạn

namespace FastFoodWeb.Models
{
    // Đổi kế thừa từ DbContext sang IdentityDbContext
    // Việc kế thừa này giúp Entity Framework tự động tạo ra các bảng: 
    // AspNetUsers, AspNetRoles, AspNetUserRoles...
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Các bảng dữ liệu món ăn bạn đã tạo từ trước
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Cực kỳ quan trọng: Phải giữ lại dòng base.OnModelCreating
            // Nếu thiếu dòng này, việc chạy Migration cho Identity sẽ bị lỗi
            base.OnModelCreating(builder);

            // Bạn có thể thêm các cấu hình Fluent API cho Product hoặc Category ở đây nếu cần
            // Ví dụ: Thiết lập giá trị mặc định, ràng buộc dữ liệu...
        }
    }
}