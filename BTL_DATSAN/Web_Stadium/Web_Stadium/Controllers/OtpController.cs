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
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='margin:0;padding:0;background:#f4f4f5;font-family:Inter,Segoe UI,sans-serif;'>
  <table width='100%' cellpadding='0' cellspacing='0'
         style='background:#f4f4f5;padding:40px 0;'>
    <tr><td align='center'>
      <table width='520' cellpadding='0' cellspacing='0'
             style='background:#ffffff;border-radius:16px;
                    overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);'>

        <!-- HEADER -->
        <tr>
          <td style='background:linear-gradient(135deg,#0ea86a,#059952);
                     padding:32px 40px;text-align:center;'>
            <div style='font-size:28px;font-weight:800;color:#ffffff;
                        letter-spacing:-0.5px;'>
              PITCH<span style='color:#86efac;'>HUB</span> ⚽
            </div>
            <div style='color:rgba(255,255,255,0.8);font-size:13px;
                        margin-top:4px;'>Nền tảng đặt sân số 1 Việt Nam</div>
          </td>
        </tr>

        <!-- BODY -->
        <tr>
          <td style='padding:40px;'>

            <!-- Icon + Title -->
            <div style='text-align:center;margin-bottom:24px;'>
              <div style='width:64px;height:64px;border-radius:50%;
                          background:#f0fdf4;border:2px solid #bbf7d0;
                          display:inline-flex;align-items:center;
                          justify-content:center;font-size:28px;
                          margin-bottom:16px;'>🔐</div>
              <h2 style='margin:0;font-size:22px;font-weight:700;
                          color:#111827;'>Xác thực tài khoản</h2>
              <p style='margin:8px 0 0;color:#6b7280;font-size:14px;'>
                Xin chào <strong style='color:#111827;'>{user.HoTen}</strong>,
                đây là mã OTP của bạn
              </p>
            </div>

            <!-- OTP Box -->
            <div style='background:linear-gradient(135deg,#f0fdf4,#dcfce7);
                        border:2px solid #86efac;border-radius:12px;
                        padding:28px;text-align:center;margin:24px 0;'>
              <div style='font-size:11px;font-weight:600;color:#16a34a;
                          text-transform:uppercase;letter-spacing:2px;
                          margin-bottom:12px;'>Mã xác thực OTP</div>
              <div style='font-size:42px;font-weight:800;
                          letter-spacing:12px;color:#0ea86a;
                          font-family:monospace;'>{maOtp}</div>
              <div style='margin-top:12px;font-size:12px;color:#6b7280;'>
                ⏱ Mã có hiệu lực trong <strong>5 phút</strong>
              </div>
            </div>

            <!-- Warning -->
            <div style='background:#fef9c3;border:1px solid #fde047;
                        border-radius:8px;padding:12px 16px;'>
              <div style='font-size:13px;color:#854d0e;line-height:1.5;'>
                ⚠️ <strong>Lưu ý bảo mật:</strong> Không chia sẻ mã này
                cho bất kỳ ai. PitchHub sẽ không bao giờ yêu cầu mã OTP
                qua điện thoại hay email khác.
              </div>
            </div>

            <!-- Note -->
            <p style='font-size:13px;color:#9ca3af;text-align:center;
                      margin-top:24px;'>
              Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email.
            </p>
          </td>
        </tr>

        <!-- FOOTER -->
        <tr>
          <td style='background:#f9fafb;padding:20px 40px;
                     border-top:1px solid #e5e7eb;text-align:center;'>
            <p style='margin:0;font-size:12px;color:#9ca3af;'>
              © 2026 PitchHub.vn ·
              <a href='#' style='color:#0ea86a;text-decoration:none;'>Hỗ trợ</a>
            </p>
          </td>
        </tr>

      </table>
    </td></tr>
  </table>
</body>
</html>";

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