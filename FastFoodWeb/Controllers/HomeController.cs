using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FastFoodWeb.Models;
using System.Diagnostics;

namespace FastFoodWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext context)
        {
            _db = context;
        }

        // CẬP NHẬT: Thêm tham số searchString để nhận từ khóa tìm kiếm từ View
        public async Task<IActionResult> Index(int? categoryId, string searchString)
        {
            // 1. Khởi tạo truy vấn lấy danh sách sản phẩm kèm thông tin Danh mục
            var productsQuery = _db.Products.Include(p => p.Category).AsQueryable();

            // 2. Lọc theo Danh mục (CategoryId) nếu người dùng nhấn vào các icon hình ảnh
            if (categoryId.HasValue && categoryId > 0)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId);
            }

            // 3. THÊM MỚI: Lọc theo tên món ăn (Tìm kiếm gần đúng)
            if (!string.IsNullOrEmpty(searchString))
            {
                // Chuyển về chữ thường (ToLower) để tìm kiếm không phân biệt hoa thường
                // Hàm Contains giúp tìm kiếm chuỗi con (vd: "gà" nằm trong "gà quay")
                string searchLower = searchString.ToLower();
                productsQuery = productsQuery.Where(p => p.Name.ToLower().Contains(searchLower));
            }

            // 4. Lưu lại từ khóa tìm kiếm để hiển thị lại trên ô nhập liệu (Input) ở giao diện
            ViewBag.CurrentSearch = searchString;

            // 5. Thực thi truy vấn và trả về danh sách kết quả cho View
            var products = await productsQuery.ToListAsync();
            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}