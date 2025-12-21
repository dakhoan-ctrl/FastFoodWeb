using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using FastFoodWeb.Models;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace FastFoodWeb.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CartController(ApplicationDbContext db)
        {
            _db = db;
        }

        // 1. Hiển thị danh sách giỏ hàng (Khách chưa đăng nhập vẫn xem được)
        public IActionResult Index()
        {
            List<CartItem> cart = GetCartItems();
            return View(cart);
        }

        // 2. Thêm sản phẩm vào giỏ hàng
        public IActionResult AddToCart(int id)
        {
            var product = _db.Products.FirstOrDefault(p => p.ProductId == id);
            if (product != null)
            {
                var cart = GetCartItems();
                var cartItem = cart.FirstOrDefault(c => c.ProductId == id);

                if (cartItem == null)
                {
                    cart.Add(new CartItem
                    {
                        ProductId = product.ProductId,
                        ProductName = product.Name,
                        Price = product.Price,
                        Quantity = 1,
                        ImageUrl = product.ImageUrl ?? ""
                    });
                }
                else
                {
                    cartItem.Quantity++;
                }
                SaveCartSession(cart);
            }
            return RedirectToAction("Index", "Home");
        }

        // 3. MUA NGAY (Yêu cầu ĐĂNG NHẬP)
        [Authorize]
        public IActionResult BuyNow(int id)
        {
            var product = _db.Products.FirstOrDefault(p => p.ProductId == id);
            if (product != null)
            {
                var cart = GetCartItems();
                var cartItem = cart.FirstOrDefault(c => c.ProductId == id);

                if (cartItem == null)
                {
                    cart.Add(new CartItem
                    {
                        ProductId = product.ProductId,
                        ProductName = product.Name,
                        Price = product.Price,
                        Quantity = 1,
                        ImageUrl = product.ImageUrl ?? ""
                    });
                }
                else
                {
                    cartItem.Quantity++;
                }
                SaveCartSession(cart);

                return RedirectToAction("Checkout");
            }
            return RedirectToAction("Index", "Home");
        }

        // 4. Cập nhật số lượng (Tăng/Giảm)
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            var cart = GetCartItems();
            var item = cart.FirstOrDefault(c => c.ProductId == id);
            if (item != null)
            {
                item.Quantity += quantity;
                if (item.Quantity <= 0) cart.Remove(item);
            }
            SaveCartSession(cart);
            return RedirectToAction("Index");
        }

        // 5. Xóa món khỏi giỏ
        public IActionResult RemoveFromCart(int id)
        {
            var cart = GetCartItems();
            var item = cart.FirstOrDefault(c => c.ProductId == id);
            if (item != null) cart.Remove(item);
            SaveCartSession(cart);
            return RedirectToAction("Index");
        }

        // 6. Thanh toán (Yêu cầu ĐĂNG NHẬP)
        [Authorize]
        public IActionResult Checkout()
        {
            var cart = GetCartItems();
            if (cart.Count == 0) return RedirectToAction("Index", "Home");
            ViewBag.Total = cart.Sum(x => x.Total);
            return View();
        }

        // 7. Xử lý lưu đơn hàng (Yêu cầu ĐĂNG NHẬP)
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(Order order)
        {
            var cart = GetCartItems();
            if (ModelState.IsValid)
            {
                order.OrderDate = DateTime.Now;
                order.TotalPrice = cart.Sum(x => x.Total);

                _db.Orders.Add(order);
                await _db.SaveChangesAsync();

                HttpContext.Session.Remove("Cart");
                return View("OrderSuccess", order);
            }
            ViewBag.Total = cart.Sum(x => x.Total);
            return View(order);
        }

        // 8. Danh sách đơn hàng cho Admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> OrderList()
        {
            var orders = await _db.Orders.OrderByDescending(o => o.OrderDate).ToListAsync();
            return View(orders);
        }

        // 9. THỐNG KÊ DOANH THU (Chỉ Admin)
        [Authorize(Roles = "Admin")]
        public IActionResult Statistics()
        {
            var orders = _db.Orders.ToList();
            var today = DateTime.Today;

            ViewBag.DailyTotal = orders
                .Where(o => o.OrderDate.Date == today)
                .Sum(o => o.TotalPrice);

            ViewBag.MonthlyTotal = orders
                .Where(o => o.OrderDate.Month == today.Month && o.OrderDate.Year == today.Year)
                .Sum(o => o.TotalPrice);

            ViewBag.YearlyTotal = orders
                .Where(o => o.OrderDate.Year == today.Year)
                .Sum(o => o.TotalPrice);

            return View();
        }

        // --- CÁC HÀM XỬ LÝ SESSION ---
        private List<CartItem> GetCartItems()
        {
            var sessionData = HttpContext.Session.GetString("Cart");
            return sessionData == null ? new List<CartItem>() : JsonConvert.DeserializeObject<List<CartItem>>(sessionData)!;
        }

        private void SaveCartSession(List<CartItem> cart)
        {
            HttpContext.Session.SetString("Cart", JsonConvert.SerializeObject(cart));
        }
    }
}