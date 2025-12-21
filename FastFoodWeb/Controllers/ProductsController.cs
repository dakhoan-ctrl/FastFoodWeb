using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FastFoodWeb.Models;

namespace FastFoodWeb.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductsController(ApplicationDbContext db, IWebHostEnvironment hostEnvironment)
        {
            _db = db;
            _hostEnvironment = hostEnvironment;
        }

        // 1. GET: Danh sách sản phẩm + Tìm kiếm gần đúng
        public async Task<IActionResult> Index(string searchString)
        {
            // Nạp kèm dữ liệu Category để hiển thị tên danh mục thay vì ID
            var productsQuery = _db.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                string searchLower = searchString.ToLower();
                // Logic: "gà" sẽ khớp với "gà quay", "gà rán" nhờ .Contains()
                productsQuery = productsQuery.Where(p => p.Name.ToLower().Contains(searchLower));
            }

            ViewBag.CurrentSearch = searchString; // Để giữ từ khóa trên ô nhập sau khi tìm
            return View(await productsQuery.ToListAsync());
        }

        // 2. GET: Mở trang thêm mới (Fix lỗi 404)
        public IActionResult Create()
        {
            // Chuẩn bị danh sách Dropdown để chọn danh mục món ăn
            ViewBag.CategoryId = new SelectList(_db.Categories, "CategoryId", "Name");
            return View();
        }

        // 3. POST: Lưu sản phẩm mới vào Database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                if (file != null)
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images/products");

                    if (!Directory.Exists(productPath)) Directory.CreateDirectory(productPath);

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    product.ImageUrl = @"/images/products/" + fileName;
                }

                _db.Products.Add(product);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // Nếu dữ liệu lỗi, nạp lại Dropdown trước khi trả về View
            ViewBag.CategoryId = new SelectList(_db.Categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        // 4. GET: Mở trang sửa sản phẩm
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id == 0) return NotFound();

            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewBag.CategoryId = new SelectList(_db.Categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        // 5. POST: Cập nhật dữ liệu sản phẩm đã sửa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? file)
        {
            if (id != product.ProductId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (file != null)
                    {
                        string wwwRootPath = _hostEnvironment.WebRootPath;
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string productPath = Path.Combine(wwwRootPath, @"images/products");

                        if (!Directory.Exists(productPath)) Directory.CreateDirectory(productPath);

                        using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }
                        product.ImageUrl = @"/images/products/" + fileName;
                    }

                    _db.Update(product);
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_db.Products.Any(e => e.ProductId == product.ProductId)) return NotFound();
                    else throw;
                }
            }
            ViewBag.CategoryId = new SelectList(_db.Categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        // 6. GET: Xác nhận xóa sản phẩm
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0) return NotFound();
            var product = await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null) return NotFound();
            return View(product);
        }

        // 7. POST: Thực hiện xóa vĩnh viễn
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product != null)
            {
                // Nếu muốn dọn dẹp bộ nhớ, bạn có thể xóa file ảnh vật lý tại đây
                _db.Products.Remove(product);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}