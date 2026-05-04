using Microsoft.AspNetCore.Mvc;
using Web_Stadium.End;
using Web_Stadium.EFCore;

namespace Web_Stadium.Controllers
{
    public class AuthController : Controller
    {
        private readonly IRepository<User> _userRepo;
        private readonly IConfiguration _config;

        public AuthController(
            IRepository<User> userRepo,
            IConfiguration config)
        {
            _userRepo = userRepo;
            _config = config;
        }

        // GET /Auth/Login
        public IActionResult Login()
        {
            if (Request.Cookies["jwt"] != null)
                return RedirectToAction("Index", "Home");
            return View();
        }

        // POST /Auth/Login
        [HttpPost]
        public async Task<IActionResult> Login(
            string email, string matkhau)
        {
            var user = await _userRepo
                .FirstOrDefaultAsync(u =>
                    u.Email == email &&
                    u.MatKhau == matkhau);

            if (user == null)
            {
                ViewBag.Error =
                    "Email hoặc mật khẩu không đúng.";
                return View();
            }

            var token = TokenHelper.TaoToken(user, _config);

            // ✅ FIX: Bỏ Secure = true khi chạy localhost HTTP
            // Secure = true chỉ dùng khi deploy HTTPS thật
            var isDev = Environment.GetEnvironmentVariable(
                "ASPNETCORE_ENVIRONMENT") == "Development";

            Response.Cookies.Append("jwt", token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.Now.AddHours(24),
                    Path = "/",
                    Secure = !isDev,      // false khi Development
                    SameSite = SameSiteMode.Lax  // Lax thay vì Strict
                });

            // Sử dụng switch để điều hướng dựa trên vai trò
            return user.VaiTro switch
            {
                "Admin" => RedirectToAction("Index", "Admin"),
                "Staff" => RedirectToAction("Index", "Staff"),
                "Owner" => RedirectToAction("Index", "Owner"),
                _ => RedirectToAction("Index", "Home") // Mặc định cho User hoặc các vai trò khác
            };
        }

        // GET /Auth/Register
        public IActionResult Register()
        {
            if (Request.Cookies["jwt"] != null)
                return RedirectToAction("Index", "Home");
            return View();
        }

        // POST /Auth/Register
        [HttpPost]
        public async Task<IActionResult> Register(
            string ho, string ten,
            string email, string matKhau,
            string soDienThoai, string vaiTro)
        {
            var existing = await _userRepo
                .FirstOrDefaultAsync(u => u.Email == email);

            if (existing != null)
            {
                ViewBag.Error = "Email này đã được đăng ký.";
                return View();
            }

            var user = new User
            {
                HoTen = $"{ho} {ten}".Trim(),
                Email = email,
                MatKhau = matKhau,
                SoDienThoai = soDienThoai,
                VaiTro = string.IsNullOrEmpty(vaiTro) ? "User" : vaiTro,
                NgayTao = DateTime.Now
            };

            await _userRepo.AddAsync(user);

            TempData["Success"] =
                "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        // GET /Auth/Logout
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt",
                new CookieOptions { Path = "/" });
            TempData["Success"] = "Đã đăng xuất thành công!";
            return RedirectToAction("Login");
        }
    }
}
