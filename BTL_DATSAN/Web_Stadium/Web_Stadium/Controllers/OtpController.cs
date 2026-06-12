using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_Stadium.EFCore;
using Web_Stadium.Filters;
using Web_Stadium.Services;

namespace Web_Stadium.Controllers
{
    [YeuCauDangNhap]
    public class OtpController : Controller
    {
        private readonly SanBongContext _context;
        private readonly IConfiguration _config;
        private readonly Web_Stadium.Services.EmailService _emailService;

        public OtpController(SanBongContext context, IConfiguration config, Web_Stadium.Services.EmailService emailService)
        {
            _context = context;
            _config = config;
            _emailService = emailService;
        }
        // Thêm hàm helper trong Controller:
        private string MaskEmail(string email)
        {
            var parts = email.Split('@');
            if (parts.Length != 2) return email;
            var name = parts[0];
            var masked = name.Length <= 2 ? name : name[0] + "***" + name[^1];
            return masked + "@" + parts[1];
        }
        private int GetUserId() => TokenHelper.LayUserId(Request, _config)!.Value;

        // ══════════════════════════════════════════════════════════
        // GET /Otp/XacThuc?returnUrl=/Booking/Create?...
        // Hiện form nhập SĐT + OTP
        // ══════════════════════════════════════════════════════════
        public async Task<IActionResult> XacThuc(string? returnUrl)
        {
            var userId = GetUserId();
            var user = await _context.Users.FindAsync(userId);

            // Đã xác thực rồi thì redirect luôn
            if (user?.DaXacThucSdt == true)
                return Redirect(returnUrl ?? "/Venues");

            ViewBag.ReturnUrl = returnUrl;
            ViewBag.SoDienThoai = user?.SoDienThoai;
            ViewBag.UserEmail = user?.Email ?? "";
            return View();
        }

        // ══════════════════════════════════════════════════════════
        // POST /Otp/GuiOtp — Sinh OTP và "gửi" (giả lập: hiện ra UI)
        // ══════════════════════════════════════════════════════════
        // POST /Otp/GuiOtp
        [HttpPost]
        public async Task<IActionResult> GuiOtp(string soDienThoai, string? returnUrl)
        {
            var userId = GetUserId();

            if (string.IsNullOrWhiteSpace(soDienThoai) || soDienThoai.Length < 9)
                return Json(new { ok = false, message = "Số điện thoại không hợp lệ." });

            // Cập nhật SĐT vào User
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Json(new { ok = false, message = "Không tìm thấy tài khoản." });

            if (user.SoDienThoai != soDienThoai.Trim())
            {
                user.SoDienThoai = soDienThoai.Trim();
                await _context.SaveChangesAsync();
            }

            // Hủy OTP cũ
            var otpCu = await _context.OtpCodes
                .Where(o => o.UserId == userId && !o.IsUsed)
                .ToListAsync();
            _context.OtpCodes.RemoveRange(otpCu);

            // Sinh OTP 6 số
            var maOtp = new Random().Next(100000, 999999).ToString();

            _context.OtpCodes.Add(new OtpCode
            {
                UserId = userId,
                SoDienThoai = soDienThoai.Trim(),
                MaOtp = maOtp,
                NgayTao = DateTime.Now,
                NgayHetHan = DateTime.Now.AddMinutes(5),
                IsUsed = false
            });
            await _context.SaveChangesAsync();

            // ✅ GỬI OTP QUA EMAIL (thay vì SMS)
            var emailBody = $@"
        <div style='font-family:Arial,sans-serif;max-width:480px;margin:0 auto;'>
            <div style='background:linear-gradient(135deg,#0f2027,#1a3a2a);padding:24px;
                        text-align:center;border-radius:12px 12px 0 0;'>
                <div style='font-size:1.6rem;font-weight:900;color:#fff;'>
                    PITCH<span style='color:#1ed760;'>HUB</span>⚽
                </div>
            </div>
            <div style='padding:24px;background:#fff;border-radius:0 0 12px 12px;'>
                <p style='color:#333;'>Xin chào <strong>{user.HoTen}</strong>,</p>
                <p style='color:#333;'>Mã OTP xác thực số điện thoại của bạn là:</p>
                <div style='background:#0f1f14;border-radius:10px;padding:16px;
                            text-align:center;margin:16px 0;'>
                    <div style='font-family:monospace;font-size:2rem;font-weight:900;
                                color:#1ed760;letter-spacing:8px;'>
                        {maOtp}
                    </div>
                </div>
                <p style='color:#888;font-size:13px;'>
                    Mã có hiệu lực trong <strong>5 phút</strong>.
                    Không chia sẻ mã này cho bất kỳ ai.
                </p>
            </div>
        </div>";

            try
            {
                await _emailService.GuiEmailAsync(
                    user.Email,
                    user.HoTen,
                    "🔐 Mã OTP xác thực PitchHub — " + maOtp,
                    emailBody
                );
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, message = "Gửi email thất bại: " + ex.Message });
            }

            return Json(new
            {
                ok = true,
                message = $"OTP đã gửi đến {MaskEmail(user.Email)}",
                emailHint = MaskEmail(user.Email)  // VD: t***@gmail.com
            });
        }

        // ══════════════════════════════════════════════════════════
        // POST /Otp/XacNhan — Kiểm tra mã OTP user nhập
        // ══════════════════════════════════════════════════════════
        [HttpPost]
        public async Task<IActionResult> XacNhan(string maOtp, string? returnUrl)
        {
            var userId = GetUserId();

            var otp = await _context.OtpCodes
                .Where(o => o.UserId == userId
                         && o.MaOtp == maOtp.Trim()
                         && !o.IsUsed
                         && o.NgayHetHan > DateTime.Now)
                .FirstOrDefaultAsync();

            if (otp == null)
            {
                TempData["Error"] = "Mã OTP không đúng hoặc đã hết hạn. Vui lòng thử lại.";
                return RedirectToAction("XacThuc", new { returnUrl });
            }

            // Đánh dấu OTP đã dùng
            otp.IsUsed = true;

            // Cập nhật User đã xác thực SĐT
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.DaXacThucSdt = true;
                user.SoDienThoai = otp.SoDienThoai;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Xác thực số điện thoại thành công! ✅";

            // Redirect về trang ban đầu hoặc trang đặt sân
            return Redirect(returnUrl ?? "/Venues");
        }
    }
}