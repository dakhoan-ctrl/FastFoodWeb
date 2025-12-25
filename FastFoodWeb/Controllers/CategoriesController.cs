using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FastFoodWeb.Models;
using Microsoft.AspNetCore.Authorization;

namespace FastFoodWeb.Controllers
{
    // Chỉ cho phép Admin truy cập vào các chức năng quản lý danh mục
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CategoriesController(ApplicationDbContext context)
        {
            _db = context;
        }

        // GET: Categories
        // Hiển thị danh sách toàn bộ danh mục (Gà Giòn, Mì Ý, Burger...)
        public async Task<IActionResult> Index()
        {
            var categories = await _db.Categories.ToListAsync();
            return View(categories); // Truyền List<Category> sang trang Index
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId,Name,Icon")] Category category) 
        {
            if (ModelState.IsValid)
            {
                _db.Add(category);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Edit/5
        // Hiển thị trang chỉnh sửa tên danh mục
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _db.Categories.FindAsync(id);
            if (category == null) return NotFound();

            return View(category); // Truyền duy nhất 1 đối tượng Category sang trang Edit
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryId,Name,Icon")] Category category)
        {
            if (id != category.CategoryId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _db.Update(category);
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_db.Categories.Any(e => e.CategoryId == category.CategoryId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Delete/5
        // Hiển thị trang xác nhận xóa danh mục
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await _db.Categories.FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null) return NotFound();

            // QUAN TRỌNG: Gửi đúng 1 đối tượng Category để View Delete.cshtml nhận
            return View(category);
        }

        // POST: Categories/Delete/5
        // Thực hiện xóa thật sự trong Database khi nhấn nút "Xóa ngay"
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category != null)
            {
                _db.Categories.Remove(category);
            }
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}