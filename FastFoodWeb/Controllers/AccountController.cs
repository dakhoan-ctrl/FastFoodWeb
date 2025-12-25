using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FastFoodWeb.Models;
using System.Threading.Tasks;

namespace FastFoodWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        public AccountController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // --- LOGIN (ĐĂNG NHẬP) ---
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            // Lưu lại đường dẫn muốn quay về (ReturnUrl) vào ViewData để truyền sang View
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    // NẾU CÓ ĐƯỜNG DẪN QUAY LẠI (Ví dụ: Quay lại trang Mua ngay)
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return LocalRedirect(returnUrl);
                    }
                    // Nếu không có thì về trang chủ
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError(string.Empty, "Đăng nhập thất bại. Sai email hoặc mật khẩu.");
            }
            return View();
        }

        // --- REGISTER (ĐĂNG KÝ) ---
        [HttpGet]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string fullName, string email, string password, string confirmPassword, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                if (password != confirmPassword)
                {
                    ModelState.AddModelError(string.Empty, "Mật khẩu xác nhận không khớp.");
                    return View();
                }

                var user = new AppUser { UserName = email, Email = email, FullName = fullName };
                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    // Đăng ký xong cũng tự động quay lại trang cũ nếu có
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return LocalRedirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View();
        }

        // --- LOGOUT (ĐĂNG XUẤT) ---
        [HttpPost]
        [Route("Account/Logout")] // Định tuyến rõ ràng để tránh lỗi
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Remove("Cart"); // Xóa session giỏ hàng cho chắc
            return RedirectToAction("Index", "Home");
        }

        // --- PROFILE ---
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            return View(user);
        }
    }
}