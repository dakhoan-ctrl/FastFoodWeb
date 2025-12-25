using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using FastFoodWeb.Models;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FastFoodWeb.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CartController(ApplicationDbContext db)
        {
            _db = db;
        }

        // 1. Xem giỏ hàng
        public IActionResult Index()
        {
            var cart = GetCartItems();
            return View(cart);
        }

        // 2. Thêm vào giỏ hàng
        public IActionResult AddToCart(int id)
        {
            var product = _db.Products.Find(id);
            if (product == null) return NotFound();

            if (User.Identity.IsAuthenticated)
            {
                // --- XỬ LÝ CHO USER ĐÃ ĐĂNG NHẬP (DATABASE) ---
                var userId = GetUserId();
                var cartItem = _db.CartItems.FirstOrDefault(c => c.UserId == userId && c.ProductId == id);

                if (cartItem != null)
                {
                    // Đã có -> Tăng số lượng
                    cartItem.Quantity++;
                    _db.CartItems.Update(cartItem);
                }
                else
                {
                    // Chưa có -> Thêm mới
                    var newItem = new CartItem
                    {
                        ProductId = product.ProductId,
                        ProductName = product.Name,
                        Price = product.Price,
                        Quantity = 1,
                        ImageUrl = product.ImageUrl ?? "",
                        UserId = userId
                    };
                    _db.CartItems.Add(newItem);
                }
                _db.SaveChanges();
            }
            else
            {
                // --- XỬ LÝ CHO KHÁCH VÃNG LAI (SESSION) ---
                var cart = GetSessionCart();
                var cartItem = cart.FirstOrDefault(c => c.ProductId == id);
                if (cartItem != null) cartItem.Quantity++;
                else
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
                SaveSessionCart(cart);
            }

            return Ok();
        }

        // 3. MUA NGAY: Yêu cầu đăng nhập ([Authorize])
        // Nếu chưa đăng nhập, hệ thống sẽ tự chuyển qua trang Login
        [Authorize]
        public IActionResult BuyNow(int id)
        {
            AddToCart(id); // Gọi hàm thêm vào giỏ
            return RedirectToAction("Checkout"); // Chuyển ngay đến trang thanh toán
        }

        // 3. Cập nhật số lượng (+/-)
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = GetUserId();
                var cartItem = _db.CartItems.FirstOrDefault(c => c.UserId == userId && c.ProductId == id);
                if (cartItem != null)
                {
                    cartItem.Quantity += quantity;
                    if (cartItem.Quantity <= 0) _db.CartItems.Remove(cartItem);
                    else _db.CartItems.Update(cartItem);
                    _db.SaveChanges();
                }
            }
            else
            {
                var cart = GetSessionCart();
                var item = cart.FirstOrDefault(c => c.ProductId == id);
                if (item != null)
                {
                    item.Quantity += quantity;
                    if (item.Quantity <= 0) cart.Remove(item);
                }
                SaveSessionCart(cart);
            }
            return RedirectToAction("Index");
        }

        // 4. Xóa sản phẩm
        public IActionResult RemoveFromCart(int id)
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = GetUserId();
                var cartItem = _db.CartItems.FirstOrDefault(c => c.UserId == userId && c.ProductId == id);
                if (cartItem != null)
                {
                    _db.CartItems.Remove(cartItem);
                    _db.SaveChanges();
                }
            }
            else
            {
                var cart = GetSessionCart();
                var item = cart.FirstOrDefault(c => c.ProductId == id);
                if (item != null)
                {
                    cart.Remove(item);
                    SaveSessionCart(cart);
                }
            }
            return RedirectToAction("Index");
        }

        // 5. Checkout
        [Authorize]
        public IActionResult Checkout()
        {
            var cart = GetCartItems();
            ViewBag.Total = cart.Sum(x => x.Total);

            var user = _db.Users.FirstOrDefault(u => u.Id == GetUserId());
            var order = new Order();
            if (user != null)
            {
                order.Name = user.FullName;
                order.Address = user.Address;
                order.PhoneNumber = user.PhoneNumber;
            }
            return View(order);
        }

        // 6. Xử lý đặt hàng
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
                // order.UserId = GetUserId(); // (Bỏ comment nếu dùng)

                _db.Orders.Add(order);
                await _db.SaveChangesAsync();

                ClearCart();
                return View("OrderSuccess", order);
            }
            ViewBag.Total = cart.Sum(x => x.Total);
            return View(order);
        }

        // --- CÁC HÀM HỖ TRỢ ---

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private List<CartItem> GetCartItems()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = GetUserId();
                return _db.CartItems.Where(c => c.UserId == userId).ToList();
            }
            return GetSessionCart();
        }

        // Tách riêng logic Session để code gọn hơn
        private List<CartItem> GetSessionCart()
        {
            var sessionCart = HttpContext.Session.GetString("Cart");
            return string.IsNullOrEmpty(sessionCart) ? new List<CartItem>() : JsonConvert.DeserializeObject<List<CartItem>>(sessionCart);
        }

        private void SaveSessionCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString("Cart", JsonConvert.SerializeObject(cart));
        }

        private void ClearCart()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = GetUserId();
                var items = _db.CartItems.Where(c => c.UserId == userId);
                _db.CartItems.RemoveRange(items);
                _db.SaveChanges();
            }
            else
            {
                HttpContext.Session.Remove("Cart");
            }
        }
    }
}