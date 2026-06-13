using System.Net;
using System.Net.Mail;

namespace Web_Stadium.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        // ── Gửi email cơ bản ─────────────────────────────────────
        public async Task GuiEmailAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            try
            {
                var host = _config["Email:SmtpHost"] ?? "smtp.gmail.com";
                var port = int.Parse(_config["Email:SmtpPort"] ?? "587");
                var sender = _config["Email:SenderEmail"]!;
                var senderName = _config["Email:SenderName"] ?? "PitchHub";
                var password = _config["Email:AppPassword"]!;

                using var client = new SmtpClient(host, port)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(sender, password)
                };

                var mail = new MailMessage
                {
                    From = new MailAddress(sender, senderName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                mail.To.Add(new MailAddress(toEmail, toName));

                await client.SendMailAsync(mail);
                _logger.LogInformation("✅ Email gửi OK → {Email} | {Subject}", toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError("❌ Lỗi gửi email → {Email}: {Msg}", toEmail, ex.Message);
            }
        }

        // ══════════════════════════════════════════════════════════
        // 1. EMAIL XÁC NHẬN ĐẶT SÂN (Owner đã duyệt)
        // ══════════════════════════════════════════════════════════
        public async Task GuiEmailXacNhanDatSan(
            string toEmail, string toName,
            string tenSan, string diaChi,
            string khungGio, string ngayThiDau,
            string maXacNhan, decimal tienCoc)
        {
            var mapsUrl = $"https://maps.google.com/?q={Uri.EscapeDataString(diaChi)}";
            var qrUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=200x200&data={maXacNhan}";

            var body = $@"
<!DOCTYPE html>
<html lang='vi'>
<head><meta charset='utf-8'></head>
<body style='font-family:Arial,sans-serif;background:#f4f4f4;margin:0;padding:20px;'>
<div style='max-width:560px;margin:0 auto;background:#fff;border-radius:16px;overflow:hidden;
            box-shadow:0 4px 20px rgba(0,0,0,.1);'>

  <!-- Header -->
  <div style='background:linear-gradient(135deg,#0f2027,#1a3a2a);padding:32px 28px;text-align:center;'>
    <div style='font-size:2rem;font-weight:900;color:#fff;letter-spacing:-1px;'>
      PITCH<span style='color:#1ed760;'>HUB</span>⚽
    </div>
    <div style='color:#1ed760;font-size:.85rem;letter-spacing:2px;margin-top:4px;'>
      ĐẶT SÂN THÀNH CÔNG
    </div>
  </div>

  <!-- Body -->
  <div style='padding:28px;'>
    <p style='color:#333;font-size:1rem;margin-bottom:20px;'>
      Xin chào <strong>{toName}</strong>,<br>
      Đơn đặt sân của bạn đã được xác nhận. Hẹn gặp bạn trên sân! ⚽
    </p>

    <!-- Info box -->
    <div style='background:#f8fffe;border:1px solid #1ed760;border-radius:12px;padding:20px;margin-bottom:20px;'>
      <table style='width:100%;border-collapse:collapse;font-size:.9rem;'>
        <tr><td style='color:#888;padding:6px 0;width:130px;'>🏟 Sân</td>
            <td style='color:#111;font-weight:700;'>{tenSan}</td></tr>
        <tr><td style='color:#888;padding:6px 0;'>📍 Địa chỉ</td>
            <td style='color:#111;'>{diaChi}</td></tr>
        <tr><td style='color:#888;padding:6px 0;'>📅 Ngày</td>
            <td style='color:#111;font-weight:600;'>{ngayThiDau}</td></tr>
        <tr><td style='color:#888;padding:6px 0;'>⏰ Giờ</td>
            <td style='color:#111;font-weight:600;'>{khungGio}</td></tr>
        <tr><td style='color:#888;padding:6px 0;'>💳 Tiền cọc</td>
            <td style='color:#1ed760;font-weight:700;'>{tienCoc:N0}đ</td></tr>
      </table>
    </div>

    <!-- Mã xác nhận -->
    <div style='background:#0f1f14;border-radius:12px;padding:18px;text-align:center;margin-bottom:20px;'>
      <div style='color:#888;font-size:.78rem;letter-spacing:1.5px;margin-bottom:8px;'>MÃ XÁC NHẬN CHECK-IN</div>
      <div style='font-family:monospace;font-size:1.6rem;font-weight:900;color:#1ed760;letter-spacing:3px;'>
        {maXacNhan}
      </div>
      <div style='color:#555;font-size:.75rem;margin-top:6px;'>Đưa mã này cho Staff khi đến sân</div>
    </div>

    <!-- QR Code -->
    <div style='text-align:center;margin-bottom:20px;'>
      <div style='color:#888;font-size:.78rem;letter-spacing:1px;margin-bottom:10px;'>HOẶC QUÉT QR CODE</div>
      <img src='{qrUrl}' width='160' height='160'
           style='border-radius:12px;border:3px solid #1ed760;' alt='QR Code' />
    </div>

    <!-- Google Maps -->
    <div style='text-align:center;margin-bottom:24px;'>
      <a href='{mapsUrl}' target='_blank'
         style='display:inline-block;background:#1ed760;color:#000;font-weight:700;
                padding:12px 28px;border-radius:10px;text-decoration:none;font-size:.9rem;'>
        📍 Chỉ đường đến sân
      </a>
    </div>

    <p style='color:#aaa;font-size:.8rem;text-align:center;'>
      Bạn sẽ nhận email nhắc lịch trước 24 giờ và 1 giờ trước trận.
    </p>
  </div>

  <!-- Footer -->
  <div style='background:#f8f8f8;padding:16px;text-align:center;border-top:1px solid #eee;'>
    <p style='color:#aaa;font-size:.75rem;margin:0;'>
      © {DateTime.Now.Year} PitchHub.vn &nbsp;·&nbsp; support@pitchhub.vn
    </p>
  </div>
</div>
</body></html>";

            await GuiEmailAsync(toEmail, toName,
                $"✅ Xác nhận đặt sân — {tenSan} | {ngayThiDau}", body);
        }

        // ══════════════════════════════════════════════════════════
        // 2. EMAIL NHẮC LỊCH (24h và 1h trước trận)
        // ══════════════════════════════════════════════════════════
        public async Task GuiEmailNhacLich(
            string toEmail, string toName,
            string tenSan, string diaChi,
            string khungGio, string ngayThiDau,
            string maXacNhan, bool la24h)
        {
            var mapsUrl = $"https://maps.google.com/?q={Uri.EscapeDataString(diaChi)}";
            var qrUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=180x180&data={maXacNhan}";
            var tieuDe = la24h ? "⏰ Nhắc lịch: Còn 24 giờ nữa là đến trận!"
                                : "🔔 Nhắc lịch: Còn 1 giờ nữa — Chuẩn bị ra sân!";
            var subtitle = la24h ? "Trận đấu của bạn diễn ra vào ngày mai!"
                                 : "Trận đấu của bạn sắp bắt đầu!";

            var body = $@"
<!DOCTYPE html>
<html lang='vi'>
<head><meta charset='utf-8'></head>
<body style='font-family:Arial,sans-serif;background:#f4f4f4;margin:0;padding:20px;'>
<div style='max-width:560px;margin:0 auto;background:#fff;border-radius:16px;overflow:hidden;
            box-shadow:0 4px 20px rgba(0,0,0,.1);'>

  <div style='background:linear-gradient(135deg,#0f2027,#1a3a2a);padding:28px;text-align:center;'>
    <div style='font-size:1.8rem;font-weight:900;color:#fff;'>PITCH<span style='color:#1ed760;'>HUB</span>⚽</div>
    <div style='color:#ffc107;font-size:.85rem;letter-spacing:2px;margin-top:4px;'>{(la24h ? "NHẮC LỊCH — 24 GIỜ" : "NHẮC LỊCH — 1 GIỜ")}</div>
  </div>

  <div style='padding:28px;'>
    <p style='color:#333;font-size:1rem;margin-bottom:20px;'>
      Xin chào <strong>{toName}</strong>,<br>
      {subtitle} Đừng quên chuẩn bị đầy đủ nhé! 💪
    </p>

    <div style='background:#f8fffe;border:1px solid #1ed760;border-radius:12px;padding:20px;margin-bottom:20px;'>
      <table style='width:100%;border-collapse:collapse;font-size:.9rem;'>
        <tr><td style='color:#888;padding:6px 0;width:130px;'>🏟 Sân</td>
            <td style='color:#111;font-weight:700;'>{tenSan}</td></tr>
        <tr><td style='color:#888;padding:6px 0;'>📍 Địa chỉ</td>
            <td style='color:#111;'>{diaChi}</td></tr>
        <tr><td style='color:#888;padding:6px 0;'>📅 Ngày</td>
            <td style='color:#111;font-weight:600;'>{ngayThiDau}</td></tr>
        <tr><td style='color:#888;padding:6px 0;'>⏰ Giờ</td>
            <td style='color:#111;font-weight:700;color:#e74c3c;'>{khungGio}</td></tr>
      </table>
    </div>

    <div style='background:#0f1f14;border-radius:12px;padding:16px;text-align:center;margin-bottom:20px;'>
      <div style='color:#888;font-size:.75rem;letter-spacing:1.5px;margin-bottom:6px;'>MÃ CHECK-IN</div>
      <div style='font-family:monospace;font-size:1.4rem;font-weight:900;color:#1ed760;letter-spacing:3px;'>{maXacNhan}</div>
    </div>

    <div style='text-align:center;margin-bottom:20px;'>
      <img src='{qrUrl}' width='140' height='140'
           style='border-radius:10px;border:3px solid #1ed760;' alt='QR' />
    </div>

    <div style='text-align:center;'>
      <a href='{mapsUrl}' target='_blank'
         style='display:inline-block;background:#1ed760;color:#000;font-weight:700;
                padding:12px 28px;border-radius:10px;text-decoration:none;font-size:.9rem;'>
        📍 Chỉ đường đến sân
      </a>
    </div>
  </div>

  <div style='background:#f8f8f8;padding:16px;text-align:center;border-top:1px solid #eee;'>
    <p style='color:#aaa;font-size:.75rem;margin:0;'>© {DateTime.Now.Year} PitchHub.vn</p>
  </div>
</div>
</body></html>";

            await GuiEmailAsync(toEmail, toName, tieuDe, body);
        }

        // ══════════════════════════════════════════════════════════
        // 3. EMAIL MỜI ĐÁNH GIÁ (30 phút sau HoanThanh)
        // ══════════════════════════════════════════════════════════
        public async Task GuiEmailMoiDanhGia(
            string toEmail, string toName,
            string tenSan, int sanBongId, int datSanId)
        {
            var linkDanhGia = $"https://pitchhub.vn/Venues/Details/{sanBongId}#danh-gia";

            var body = $@"
<!DOCTYPE html>
<html lang='vi'>
<head><meta charset='utf-8'></head>
<body style='font-family:Arial,sans-serif;background:#f4f4f4;margin:0;padding:20px;'>
<div style='max-width:560px;margin:0 auto;background:#fff;border-radius:16px;overflow:hidden;
            box-shadow:0 4px 20px rgba(0,0,0,.1);'>

  <div style='background:linear-gradient(135deg,#0f2027,#1a3a2a);padding:28px;text-align:center;'>
    <div style='font-size:1.8rem;font-weight:900;color:#fff;'>PITCH<span style='color:#1ed760;'>HUB</span>⚽</div>
    <div style='color:#ffc107;font-size:.85rem;letter-spacing:2px;margin-top:4px;'>TRẬN ĐẤU ĐÃ KẾT THÚC</div>
  </div>

  <div style='padding:28px;text-align:center;'>
    <div style='font-size:3rem;margin-bottom:12px;'>⭐</div>
    <h2 style='color:#111;font-size:1.2rem;margin-bottom:12px;'>
      Bạn cảm thấy thế nào về <strong>{tenSan}</strong>?
    </h2>
    <p style='color:#666;font-size:.9rem;margin-bottom:24px;line-height:1.6;'>
      Đánh giá của bạn giúp cộng đồng PitchHub chọn được sân tốt hơn.<br>
      Chỉ mất 30 giây và bạn nhận ngay <strong style='color:#ffc107;'>+5 điểm thưởng ⭐</strong>
    </p>

    <a href='{linkDanhGia}' target='_blank'
       style='display:inline-block;background:#1ed760;color:#000;font-weight:700;
              padding:14px 36px;border-radius:12px;text-decoration:none;font-size:1rem;
              box-shadow:0 4px 15px rgba(30,215,96,.3);'>
      ⭐ Đánh giá ngay
    </a>

    <p style='color:#aaa;font-size:.75rem;margin-top:20px;'>
      Đánh giá theo 3 tiêu chí: Chất lượng cỏ · Cơ sở vật chất · Thái độ nhân viên
    </p>
  </div>

  <div style='background:#f8f8f8;padding:16px;text-align:center;border-top:1px solid #eee;'>
    <p style='color:#aaa;font-size:.75rem;margin:0;'>© {DateTime.Now.Year} PitchHub.vn</p>
  </div>
</div>
</body></html>";

            await GuiEmailAsync(toEmail, toName,
                $"⭐ Đánh giá {tenSan} và nhận +5 điểm thưởng!", body);
        }

        // ══════════════════════════════════════════════════════════
        // 5. EMAIL HỦY ĐƠN TỰ ĐỘNG (không check-in sau 2 tiếng)
        // ══════════════════════════════════════════════════════════
        public async Task GuiEmailHuyDonTuDong(
            string toEmail, string toName,
            string tenSan, DateTime ngayThiDau,
            string gioBatDau, string gioKetThuc,
            string maXacNhan)
        {
            var ngayStr = ngayThiDau.ToString("dd/MM/yyyy");
            var body = $@"
<!DOCTYPE html>
<html lang='vi'>
<head><meta charset='utf-8'></head>
<body style='font-family:Arial,sans-serif;background:#f4f4f4;margin:0;padding:20px;'>
<div style='max-width:560px;margin:0 auto;background:#fff;border-radius:16px;overflow:hidden;
            box-shadow:0 4px 20px rgba(0,0,0,.1);'>

  <div style='background:linear-gradient(135deg,#1a0a0a,#2a1010);padding:28px;text-align:center;'>
    <div style='font-size:1.8rem;font-weight:900;color:#fff;'>PITCH<span style='color:#1ed760;'>HUB</span>⚽</div>
    <div style='color:#e74c3c;font-size:.85rem;letter-spacing:2px;margin-top:4px;'>ĐƠN ĐẶT SÂN ĐÃ BỊ HỦY</div>
  </div>

  <div style='padding:28px;'>
    <p style='color:#333;'>Xin chào <strong>{toName}</strong>,</p>
    <p style='color:#333;line-height:1.6;'>
      Đơn đặt sân <strong style='color:#e74c3c;'>{maXacNhan}</strong> của bạn đã bị hủy tự động
      vì không có check-in trong vòng 2 tiếng kể từ giờ bắt đầu.
    </p>

    <div style='background:#fff8f0;border:1px solid #e74c3c;border-radius:12px;padding:20px;margin:20px 0;'>
      <table style='width:100%;border-collapse:collapse;font-size:.9rem;'>
        <tr><td style='color:#888;padding:6px 0;width:130px;'>🏟 Sân</td>
            <td style='color:#111;font-weight:700;'>{tenSan}</td></tr>
        <tr><td style='color:#888;padding:6px 0;'>📅 Ngày</td>
            <td style='color:#111;font-weight:600;'>{ngayStr}</td></tr>
        <tr><td style='color:#888;padding:6px 0;'>⏰ Khung giờ</td>
            <td style='color:#111;font-weight:600;'>{gioBatDau} – {gioKetThuc}</td></tr>
        <tr><td style='color:#888;padding:6px 0;'>❌ Lý do</td>
            <td style='color:#e74c3c;'>Không check-in sau 2 tiếng</td></tr>
      </table>
    </div>

    <div style='background:#f8fffe;border:1px solid #1ed760;border-radius:12px;padding:18px;text-align:center;'>
      <div style='font-size:1.2rem;margin-bottom:8px;'>⚽ Muốn đặt sân khác?</div>
      <p style='color:#555;font-size:.85rem;margin:0;line-height:1.6;'>
        Bạn có thể đặt lại sân bất kỳ lúc nào trên PitchHub.<br>
        Nếu có thắc mắc, liên hệ <a href='mailto:support@pitchhub.vn' style='color:#1ed760;'>support@pitchhub.vn</a>
      </p>
    </div>
  </div>

  <div style='background:#f8f8f8;padding:16px;text-align:center;border-top:1px solid #eee;'>
    <p style='color:#aaa;font-size:.75rem;margin:0;'>© {DateTime.Now.Year} PitchHub.vn</p>
  </div>
</div>
</body></html>";

            await GuiEmailAsync(toEmail, toName,
                $"[PitchHub] Đơn đặt sân {maXacNhan} đã bị hủy tự động", body);
        }

        // ══════════════════════════════════════════════════════════
        // 6. EMAIL NHẮC CHECK-IN (30 phút trước giờ bắt đầu)
        // ══════════════════════════════════════════════════════════
        public async Task GuiEmailNhacCheckIn(
            string toEmail, string toName,
            string tenSan, DateTime ngayThiDau,
            string gioBatDau, string maXacNhan)
        {
            var ngayStr = ngayThiDau.ToString("dd/MM/yyyy");
            var qrUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=180x180&data={maXacNhan}";

            var body = $@"
<!DOCTYPE html>
<html lang='vi'>
<head><meta charset='utf-8'></head>
<body style='font-family:Arial,sans-serif;background:#f4f4f4;margin:0;padding:20px;'>
<div style='max-width:560px;margin:0 auto;background:#fff;border-radius:16px;overflow:hidden;
            box-shadow:0 4px 20px rgba(0,0,0,.1);'>

  <div style='background:linear-gradient(135deg,#0f2027,#1a3a2a);padding:28px;text-align:center;'>
    <div style='font-size:1.8rem;font-weight:900;color:#fff;'>PITCH<span style='color:#1ed760;'>HUB</span>⚽</div>
    <div style='color:#ffc107;font-size:.85rem;letter-spacing:2px;margin-top:4px;'>SẮP ĐẾN GIỜ CHECK-IN!</div>
  </div>

  <div style='padding:28px;'>
    <div style='font-size:2.5rem;text-align:center;margin-bottom:12px;'>⏰</div>
    <p style='color:#333;font-size:1rem;text-align:center;margin-bottom:20px;'>
      Xin chào <strong>{toName}</strong>,<br>
      <span style='color:#e74c3c;font-weight:700;'>Còn khoảng 30 phút</span> nữa là đến giờ đá của bạn!<br>
      Hãy chuẩn bị ra sân nhé 💪
    </p>

    <div style='background:#f8fffe;border:1px solid #1ed760;border-radius:12px;padding:20px;margin-bottom:20px;'>
      <table style='width:100%;border-collapse:collapse;font-size:.9rem;'>
        <tr><td style='color:#888;padding:6px 0;width:130px;'>🏟 Sân</td>
            <td style='color:#111;font-weight:700;'>{tenSan}</td></tr>
        <tr><td style='color:#888;padding:6px 0;'>📅 Ngày</td>
            <td style='color:#111;font-weight:600;'>{ngayStr}</td></tr>
        <tr><td style='color:#888;padding:6px 0;'>⏰ Giờ bắt đầu</td>
            <td style='color:#e74c3c;font-weight:700;font-size:1rem;'>{gioBatDau}</td></tr>
      </table>
    </div>

    <div style='background:#0f1f14;border-radius:12px;padding:18px;text-align:center;margin-bottom:20px;'>
      <div style='color:#888;font-size:.75rem;letter-spacing:1.5px;margin-bottom:8px;'>MÃ CHECK-IN CỦA BẠN</div>
      <div style='font-family:monospace;font-size:1.5rem;font-weight:900;color:#1ed760;letter-spacing:3px;'>
        {maXacNhan}
      </div>
      <div style='color:#555;font-size:.75rem;margin-top:6px;'>Đưa mã hoặc QR code này cho Staff khi đến sân</div>
    </div>

    <div style='text-align:center;margin-bottom:20px;'>
      <img src='{qrUrl}' width='140' height='140'
           style='border-radius:10px;border:3px solid #1ed760;' alt='QR Check-in' />
    </div>

    <div style='background:#fff3cd;border:1px solid #ffc107;border-radius:10px;padding:14px;text-align:center;'>
      <p style='color:#856404;font-size:.85rem;margin:0;'>
        ⚠️ Vui lòng check-in trong vòng <strong>2 tiếng</strong> kể từ giờ bắt đầu.<br>
        Quá thời gian, đơn sẽ bị hủy tự động.
      </p>
    </div>
  </div>

  <div style='background:#f8f8f8;padding:16px;text-align:center;border-top:1px solid #eee;'>
    <p style='color:#aaa;font-size:.75rem;margin:0;'>© {DateTime.Now.Year} PitchHub.vn &nbsp;·&nbsp; support@pitchhub.vn</p>
  </div>
</div>
</body></html>";

            await GuiEmailAsync(toEmail, toName,
                $"[PitchHub] Nhắc nhở: Còn 30 phút nữa đến giờ check-in tại {tenSan}!", body);
        }

        // ══════════════════════════════════════════════════════════
        // 4. EMAIL THÔNG BÁO HỦY ĐƠN
        // ══════════════════════════════════════════════════════════
        public async Task GuiEmailHuyDon(
            string toEmail, string toName,
            string tenSan, string ngayThiDau,
            string lyDo, decimal soTienHoan)
        {
            var body = $@"
<!DOCTYPE html>
<html lang='vi'>
<head><meta charset='utf-8'></head>
<body style='font-family:Arial,sans-serif;background:#f4f4f4;margin:0;padding:20px;'>
<div style='max-width:560px;margin:0 auto;background:#fff;border-radius:16px;overflow:hidden;
            box-shadow:0 4px 20px rgba(0,0,0,.1);'>

  <div style='background:linear-gradient(135deg,#1a0a0a,#2a1010);padding:28px;text-align:center;'>
    <div style='font-size:1.8rem;font-weight:900;color:#fff;'>PITCH<span style='color:#1ed760;'>HUB</span>⚽</div>
    <div style='color:#e74c3c;font-size:.85rem;letter-spacing:2px;margin-top:4px;'>THÔNG BÁO HỦY ĐƠN</div>
  </div>

  <div style='padding:28px;'>
    <p style='color:#333;'>Xin chào <strong>{toName}</strong>,</p>
    <p style='color:#333;'>Đơn đặt sân <strong>{tenSan}</strong> ngày <strong>{ngayThiDau}</strong> đã bị hủy.</p>
    <p style='color:#666;'>Lý do: {lyDo}</p>

    <div style='background:#fff8f0;border:1px solid #f59e0b;border-radius:12px;padding:18px;margin:20px 0;text-align:center;'>
      <div style='color:#888;font-size:.8rem;margin-bottom:6px;'>SỐ TIỀN HOÀN TRẢ</div>
      <div style='font-size:1.8rem;font-weight:900;color:#f59e0b;'>{soTienHoan:N0}đ</div>
      <div style='color:#aaa;font-size:.75rem;margin-top:6px;'>Hoàn về phương thức thanh toán ban đầu trong 1–3 ngày làm việc</div>
    </div>

    <p style='color:#888;font-size:.85rem;'>
      Nếu có thắc mắc, vui lòng liên hệ <a href='mailto:support@pitchhub.vn'>support@pitchhub.vn</a>
    </p>
  </div>

  <div style='background:#f8f8f8;padding:16px;text-align:center;border-top:1px solid #eee;'>
    <p style='color:#aaa;font-size:.75rem;margin:0;'>© {DateTime.Now.Year} PitchHub.vn</p>
  </div>
</div>
</body></html>";

            await GuiEmailAsync(toEmail, toName,
                $"❌ Đơn đặt sân {tenSan} đã bị hủy", body);
        }

        // ══════════════════════════════════════════════════════════
        // 5. THÔNG BÁO YÊU CẦU ĐỔI GIỜ MỚI → STAFF
        // ══════════════════════════════════════════════════════════
        public async Task GuiEmailYeuCauMoiChoStaff(
            string staffEmail, string staffName,
            string tenKhach, string tenSan,
            string gioMoi, string ngayMoi, string lyDo)
        {
            var body = $@"
<!DOCTYPE html><html lang='vi'><head><meta charset='utf-8'></head>
<body style='font-family:Arial,sans-serif;background:#f4f4f4;margin:0;padding:20px;'>
<div style='max-width:560px;margin:0 auto;background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,.1);'>
  <div style='background:linear-gradient(135deg,#0f2027,#1a3a2a);padding:28px;text-align:center;'>
    <div style='font-size:1.8rem;font-weight:900;color:#fff;'>PITCH<span style='color:#1ed760;'>HUB</span>⚽</div>
    <div style='color:#ffc107;font-size:.85rem;letter-spacing:2px;margin-top:4px;'>YÊU CẦU ĐỔI KHUNG GIỜ</div>
  </div>
  <div style='padding:28px;'>
    <p style='color:#333;'>Xin chào <strong>{staffName}</strong>,</p>
    <p style='color:#555;'>Khách hàng <strong>{tenKhach}</strong> vừa gửi yêu cầu đổi khung giờ tại sân của bạn. Vui lòng xem xét và xử lý.</p>
    <div style='background:#fffde7;border:1px solid #ffc107;border-radius:12px;padding:20px;margin:20px 0;'>
      <table style='width:100%;font-size:.9rem;border-collapse:collapse;'>
        <tr><td style='color:#888;padding:5px 0;width:130px;'>🏟 Sân</td><td style='color:#111;font-weight:700;'>{tenSan}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>📅 Ngày mới</td><td style='color:#111;font-weight:600;'>{ngayMoi}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>⏰ Khung giờ mới</td><td style='color:#e65100;font-weight:700;'>{gioMoi}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>💬 Lý do</td><td style='color:#333;font-style:italic;'>{lyDo}</td></tr>
      </table>
    </div>
    <p style='color:#555;font-size:.9rem;'>Hãy đăng nhập vào hệ thống để xử lý yêu cầu này (Chuyển Owner duyệt hoặc Từ chối).</p>
  </div>
  <div style='background:#f8f8f8;padding:14px;text-align:center;border-top:1px solid #eee;'>
    <p style='color:#aaa;font-size:.75rem;margin:0;'>© {DateTime.Now.Year} PitchHub.vn</p>
  </div>
</div>
</body></html>";
            await GuiEmailAsync(staffEmail, staffName,
                $"[PitchHub] Yêu cầu đổi giờ từ {tenKhach} — {tenSan}", body);
        }

        // ══════════════════════════════════════════════════════════
        // 6. THÔNG BÁO CHUYỂN OWNER XEM XÉT
        // ══════════════════════════════════════════════════════════
        public async Task GuiEmailChuyenChoOwner(
            string ownerEmail, string ownerName,
            string tenKhach, string tenSan,
            string gioMoi, string ngayMoi, string ghiChuStaff)
        {
            var body = $@"
<!DOCTYPE html><html lang='vi'><head><meta charset='utf-8'></head>
<body style='font-family:Arial,sans-serif;background:#f4f4f4;margin:0;padding:20px;'>
<div style='max-width:560px;margin:0 auto;background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,.1);'>
  <div style='background:linear-gradient(135deg,#0f2027,#1a3a2a);padding:28px;text-align:center;'>
    <div style='font-size:1.8rem;font-weight:900;color:#fff;'>PITCH<span style='color:#1ed760;'>HUB</span>⚽</div>
    <div style='color:#1ed760;font-size:.85rem;letter-spacing:2px;margin-top:4px;'>YÊU CẦU ĐỔI GIỜ — CHỜ PHÊ DUYỆT</div>
  </div>
  <div style='padding:28px;'>
    <p style='color:#333;'>Xin chào <strong>{ownerName}</strong>,</p>
    <p style='color:#555;'>Staff đã chuyển yêu cầu đổi giờ của khách hàng <strong>{tenKhach}</strong> để bạn phê duyệt.</p>
    <div style='background:#f8fffe;border:1px solid #1ed760;border-radius:12px;padding:20px;margin:20px 0;'>
      <table style='width:100%;font-size:.9rem;border-collapse:collapse;'>
        <tr><td style='color:#888;padding:5px 0;width:130px;'>🏟 Sân</td><td style='color:#111;font-weight:700;'>{tenSan}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>📅 Ngày mới</td><td style='color:#111;font-weight:600;'>{ngayMoi}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>⏰ Khung giờ mới</td><td style='color:#0EA86A;font-weight:700;'>{gioMoi}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>📝 Ghi chú Staff</td><td style='color:#333;font-style:italic;'>{ghiChuStaff}</td></tr>
      </table>
    </div>
    <p style='color:#555;font-size:.9rem;'>Đăng nhập vào Owner Portal để phê duyệt hoặc từ chối yêu cầu này.</p>
  </div>
  <div style='background:#f8f8f8;padding:14px;text-align:center;border-top:1px solid #eee;'>
    <p style='color:#aaa;font-size:.75rem;margin:0;'>© {DateTime.Now.Year} PitchHub.vn</p>
  </div>
</div>
</body></html>";
            await GuiEmailAsync(ownerEmail, ownerName,
                $"[PitchHub] Yêu cầu đổi giờ cần phê duyệt — {tenSan}", body);
        }

        // ══════════════════════════════════════════════════════════
        // 7. XÁC NHẬN ĐỔI GIỜ THÀNH CÔNG → KHÁCH
        // ══════════════════════════════════════════════════════════
        public async Task GuiEmailDoiGioPheDuyet(
            string userEmail, string userName,
            string tenSan, string gioMoi, string ngayMoi, string maXacNhan)
        {
            var qrUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=180x180&data={maXacNhan}";
            var body = $@"
<!DOCTYPE html><html lang='vi'><head><meta charset='utf-8'></head>
<body style='font-family:Arial,sans-serif;background:#f4f4f4;margin:0;padding:20px;'>
<div style='max-width:560px;margin:0 auto;background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,.1);'>
  <div style='background:linear-gradient(135deg,#0f2027,#1a3a2a);padding:28px;text-align:center;'>
    <div style='font-size:1.8rem;font-weight:900;color:#fff;'>PITCH<span style='color:#1ed760;'>HUB</span>⚽</div>
    <div style='color:#1ed760;font-size:.85rem;letter-spacing:2px;margin-top:4px;'>ĐỔI KHUNG GIỜ THÀNH CÔNG ✅</div>
  </div>
  <div style='padding:28px;'>
    <p style='color:#333;'>Xin chào <strong>{userName}</strong>,</p>
    <p style='color:#555;'>Yêu cầu đổi khung giờ đặt sân của bạn đã được Owner phê duyệt! Chi tiết đơn mới:</p>
    <div style='background:#f8fffe;border:1px solid #1ed760;border-radius:12px;padding:20px;margin:20px 0;'>
      <table style='width:100%;font-size:.9rem;border-collapse:collapse;'>
        <tr><td style='color:#888;padding:5px 0;width:130px;'>🏟 Sân</td><td style='color:#111;font-weight:700;'>{tenSan}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>📅 Ngày mới</td><td style='color:#0EA86A;font-weight:700;'>{ngayMoi}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>⏰ Khung giờ mới</td><td style='color:#0EA86A;font-weight:700;'>{gioMoi}</td></tr>
      </table>
    </div>
    <div style='background:#0f1f14;border-radius:12px;padding:18px;text-align:center;margin-bottom:20px;'>
      <div style='color:#888;font-size:.75rem;letter-spacing:1.5px;margin-bottom:8px;'>MÃ CHECK-IN</div>
      <div style='font-family:monospace;font-size:1.5rem;font-weight:900;color:#1ed760;letter-spacing:3px;'>{maXacNhan}</div>
    </div>
    <div style='text-align:center;margin-bottom:16px;'>
      <img src='{qrUrl}' width='140' height='140' style='border-radius:10px;border:3px solid #1ed760;' />
    </div>
  </div>
  <div style='background:#f8f8f8;padding:14px;text-align:center;border-top:1px solid #eee;'>
    <p style='color:#aaa;font-size:.75rem;margin:0;'>© {DateTime.Now.Year} PitchHub.vn</p>
  </div>
</div>
</body></html>";
            await GuiEmailAsync(userEmail, userName,
                $"[PitchHub] ✅ Đổi khung giờ thành công — {tenSan}", body);
        }

        // ══════════════════════════════════════════════════════════
        // 8. THÔNG BÁO TỪ CHỐI ĐỔI GIỜ → KHÁCH
        // ══════════════════════════════════════════════════════════
        public async Task GuiEmailDoiGioTuChoi(
            string userEmail, string userName,
            string tenSan, string lyDo)
        {
            var body = $@"
<!DOCTYPE html><html lang='vi'><head><meta charset='utf-8'></head>
<body style='font-family:Arial,sans-serif;background:#f4f4f4;margin:0;padding:20px;'>
<div style='max-width:560px;margin:0 auto;background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,.1);'>
  <div style='background:linear-gradient(135deg,#7f0000,#1a0000);padding:28px;text-align:center;'>
    <div style='font-size:1.8rem;font-weight:900;color:#fff;'>PITCH<span style='color:#ef4444;'>HUB</span>⚽</div>
    <div style='color:#ef4444;font-size:.85rem;letter-spacing:2px;margin-top:4px;'>YÊU CẦU ĐỔI GIỜ BỊ TỪ CHỐI</div>
  </div>
  <div style='padding:28px;'>
    <p style='color:#333;'>Xin chào <strong>{userName}</strong>,</p>
    <p style='color:#555;'>Rất tiếc, yêu cầu đổi khung giờ tại sân <strong>{tenSan}</strong> của bạn không được chấp thuận.</p>
    <div style='background:#fff5f5;border:1px solid #ef4444;border-radius:12px;padding:16px;margin:20px 0;'>
      <div style='color:#888;font-size:.8rem;margin-bottom:6px;'>Lý do từ chối:</div>
      <div style='color:#333;font-style:italic;'>{lyDo}</div>
    </div>
    <p style='color:#555;font-size:.9rem;'>Đơn đặt sân gốc của bạn vẫn được giữ nguyên. Nếu có thắc mắc, vui lòng liên hệ hỗ trợ.</p>
  </div>
  <div style='background:#f8f8f8;padding:14px;text-align:center;border-top:1px solid #eee;'>
    <p style='color:#aaa;font-size:.75rem;margin:0;'>© {DateTime.Now.Year} PitchHub.vn</p>
  </div>
</div>
</body></html>";
            await GuiEmailAsync(userEmail, userName,
                $"[PitchHub] Yêu cầu đổi giờ tại {tenSan} không được chấp thuận", body);
        }

        // ══════════════════════════════════════════════════════════
        // 9. THÔNG BÁO YÊU CẦU ĐỔI SÂN → OWNER SÂN MỚI
        // ══════════════════════════════════════════════════════════
        public async Task GuiEmailYeuCauDoiSanChoOwner(
            string ownerEmail, string ownerName,
            string tenKhach, string tenSanCu, string tenSanMoi,
            string gioMoi, string ngay, string lyDo, decimal chenhLechGia)
        {
            var clSign = chenhLechGia >= 0 ? "+" : "";
            var body = $@"
<!DOCTYPE html><html lang='vi'><head><meta charset='utf-8'></head>
<body style='font-family:Arial,sans-serif;background:#f4f4f4;margin:0;padding:20px;'>
<div style='max-width:560px;margin:0 auto;background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,.1);'>
  <div style='background:linear-gradient(135deg,#0f2027,#1a3a2a);padding:28px;text-align:center;'>
    <div style='font-size:1.8rem;font-weight:900;color:#fff;'>PITCH<span style='color:#1ed760;'>HUB</span>⚽</div>
    <div style='color:#60a5fa;font-size:.85rem;letter-spacing:2px;margin-top:4px;'>YÊU CẦU ĐỔI SÂN</div>
  </div>
  <div style='padding:28px;'>
    <p style='color:#333;'>Xin chào <strong>{ownerName}</strong>,</p>
    <p style='color:#555;'>Khách hàng <strong>{tenKhach}</strong> muốn chuyển đặt sân từ <strong>{tenSanCu}</strong> sang <strong>{tenSanMoi}</strong> của bạn. Vui lòng xem xét và phê duyệt.</p>
    <div style='background:#eff6ff;border:1px solid #60a5fa;border-radius:12px;padding:20px;margin:20px 0;'>
      <table style='width:100%;font-size:.9rem;border-collapse:collapse;'>
        <tr><td style='color:#888;padding:5px 0;width:140px;'>🏟 Sân cũ</td><td style='color:#555;'>{tenSanCu}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>🏟 Sân mới</td><td style='color:#111;font-weight:700;'>{tenSanMoi}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>📅 Ngày</td><td style='color:#111;font-weight:600;'>{ngay}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>⏰ Khung giờ</td><td style='color:#111;font-weight:600;'>{gioMoi}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>💰 Chênh lệch giá</td>
            <td style='color:{(chenhLechGia >= 0 ? "#ef4444" : "#22c55e")};font-weight:700;'>{clSign}{chenhLechGia:N0}đ</td></tr>
        <tr><td style='color:#888;padding:5px 0;vertical-align:top;'>💬 Lý do</td><td style='color:#333;font-style:italic;'>{lyDo}</td></tr>
      </table>
    </div>
    <p style='color:#555;font-size:.9rem;'>Đăng nhập Owner Portal để phê duyệt hoặc từ chối yêu cầu này.</p>
  </div>
  <div style='background:#f8f8f8;padding:14px;text-align:center;border-top:1px solid #eee;'>
    <p style='color:#aaa;font-size:.75rem;margin:0;'>© {DateTime.Now.Year} PitchHub.vn</p>
  </div>
</div>
</body></html>";
            await GuiEmailAsync(ownerEmail, ownerName,
                $"[PitchHub] Yêu cầu đổi sân mới từ {tenKhach} — {tenSanMoi}", body);
        }

        // ══════════════════════════════════════════════════════════
        // 10. XÁC NHẬN ĐỔI SÂN THÀNH CÔNG → KHÁCH
        // ══════════════════════════════════════════════════════════
        public async Task GuiEmailDoiSanPheDuyet(
            string userEmail, string userName,
            string tenSanCu, string tenSanMoi,
            string gioMoi, string ngay, string maXacNhan, decimal chenhLechGia)
        {
            var qrUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=180x180&data={maXacNhan}";
            var clSign = chenhLechGia >= 0 ? "+" : "";
            var clNote = chenhLechGia > 0
                ? $"Bạn cần thanh toán thêm <strong style='color:#ef4444;'>{chenhLechGia:N0}đ</strong> chênh lệch."
                : chenhLechGia < 0
                    ? $"Bạn sẽ được hoàn lại <strong style='color:#22c55e;'>{Math.Abs(chenhLechGia):N0}đ</strong>."
                    : "Không có chênh lệch giá.";
            var body = $@"
<!DOCTYPE html><html lang='vi'><head><meta charset='utf-8'></head>
<body style='font-family:Arial,sans-serif;background:#f4f4f4;margin:0;padding:20px;'>
<div style='max-width:560px;margin:0 auto;background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,.1);'>
  <div style='background:linear-gradient(135deg,#0f2027,#1a3a2a);padding:28px;text-align:center;'>
    <div style='font-size:1.8rem;font-weight:900;color:#fff;'>PITCH<span style='color:#1ed760;'>HUB</span>⚽</div>
    <div style='color:#1ed760;font-size:.85rem;letter-spacing:2px;margin-top:4px;'>ĐỔI SÂN THÀNH CÔNG ✅</div>
  </div>
  <div style='padding:28px;'>
    <p style='color:#333;'>Xin chào <strong>{userName}</strong>,</p>
    <p style='color:#555;'>Yêu cầu đổi sân của bạn đã được phê duyệt!</p>
    <div style='background:#f8fffe;border:1px solid #1ed760;border-radius:12px;padding:20px;margin:20px 0;'>
      <table style='width:100%;font-size:.9rem;border-collapse:collapse;'>
        <tr><td style='color:#888;padding:5px 0;width:130px;'>🏟 Sân cũ</td><td style='color:#555;text-decoration:line-through;'>{tenSanCu}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>🏟 Sân mới</td><td style='color:#0EA86A;font-weight:700;'>{tenSanMoi}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>📅 Ngày</td><td style='color:#0EA86A;font-weight:700;'>{ngay}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>⏰ Khung giờ</td><td style='color:#0EA86A;font-weight:700;'>{gioMoi}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>💰 Chênh lệch</td><td style='font-weight:700;'>{clSign}{chenhLechGia:N0}đ</td></tr>
      </table>
    </div>
    <p style='color:#555;font-size:.9rem;'>{clNote}</p>
    <div style='background:#0f1f14;border-radius:12px;padding:18px;text-align:center;margin-bottom:16px;'>
      <div style='color:#888;font-size:.75rem;letter-spacing:1.5px;margin-bottom:8px;'>MÃ CHECK-IN MỚI</div>
      <div style='font-family:monospace;font-size:1.5rem;font-weight:900;color:#1ed760;letter-spacing:3px;'>{maXacNhan}</div>
    </div>
    <div style='text-align:center;'>
      <img src='{qrUrl}' width='140' height='140' style='border-radius:10px;border:3px solid #1ed760;' />
    </div>
  </div>
  <div style='background:#f8f8f8;padding:14px;text-align:center;border-top:1px solid #eee;'>
    <p style='color:#aaa;font-size:.75rem;margin:0;'>© {DateTime.Now.Year} PitchHub.vn</p>
  </div>
</div>
</body></html>";
            await GuiEmailAsync(userEmail, userName,
                $"[PitchHub] ✅ Đổi sân thành công — {tenSanMoi}", body);
        }

        // ══════════════════════════════════════════════════════════
        // 11. TỪ CHỐI ĐỔI SÂN → KHÁCH
        // ══════════════════════════════════════════════════════════
        public async Task GuiEmailDoiSanTuChoi(
            string userEmail, string userName,
            string tenSanMoi, string lyDo)
        {
            var body = $@"
<!DOCTYPE html><html lang='vi'><head><meta charset='utf-8'></head>
<body style='font-family:Arial,sans-serif;background:#f4f4f4;margin:0;padding:20px;'>
<div style='max-width:560px;margin:0 auto;background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,.1);'>
  <div style='background:linear-gradient(135deg,#7f0000,#1a0000);padding:28px;text-align:center;'>
    <div style='font-size:1.8rem;font-weight:900;color:#fff;'>PITCH<span style='color:#ef4444;'>HUB</span>⚽</div>
    <div style='color:#ef4444;font-size:.85rem;letter-spacing:2px;margin-top:4px;'>YÊU CẦU ĐỔI SÂN BỊ TỪ CHỐI</div>
  </div>
  <div style='padding:28px;'>
    <p style='color:#333;'>Xin chào <strong>{userName}</strong>,</p>
    <p style='color:#555;'>Rất tiếc, yêu cầu chuyển sang sân <strong>{tenSanMoi}</strong> không được chấp thuận.</p>
    <div style='background:#fff5f5;border:1px solid #ef4444;border-radius:12px;padding:16px;margin:20px 0;'>
      <div style='color:#888;font-size:.8rem;margin-bottom:6px;'>Lý do từ chối:</div>
      <div style='color:#333;font-style:italic;'>{lyDo}</div>
    </div>
    <p style='color:#555;font-size:.9rem;'>Đơn đặt sân gốc của bạn vẫn được giữ nguyên. Nếu cần hỗ trợ, liên hệ support@pitchhub.vn.</p>
  </div>
  <div style='background:#f8f8f8;padding:14px;text-align:center;border-top:1px solid #eee;'>
    <p style='color:#aaa;font-size:.75rem;margin:0;'>© {DateTime.Now.Year} PitchHub.vn</p>
  </div>
</div>
</body></html>";
            await GuiEmailAsync(userEmail, userName,
                $"[PitchHub] Yêu cầu đổi sang sân {tenSanMoi} không được chấp thuận", body);
        }

        // ══════════════════════════════════════════════════════════
        // 12. LỜI MỜI GHÉP TRẬN → USER B
        // ══════════════════════════════════════════════════════════
        public async Task GuiEmailLoiMoiGhepTran(
            string emailB, string tenUserB,
            string tenUserA, string tenSanA,
            string ngay, string gio, string loiNhan)
        {
            var body = $@"
<!DOCTYPE html><html lang='vi'><head><meta charset='utf-8'></head>
<body style='font-family:Arial,sans-serif;background:#f4f4f4;margin:0;padding:20px;'>
<div style='max-width:560px;margin:0 auto;background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,.1);'>
  <div style='background:linear-gradient(135deg,#0f2027,#1a3a2a);padding:28px;text-align:center;'>
    <div style='font-size:1.8rem;font-weight:900;color:#fff;'>PITCH<span style='color:#1ed760;'>HUB</span>⚽</div>
    <div style='color:#1ed760;font-size:.85rem;letter-spacing:2px;margin-top:4px;'>LỜI MỜI GHÉP TRẬN ⚽</div>
  </div>
  <div style='padding:28px;'>
    <p style='color:#333;'>Xin chào <strong>{tenUserB}</strong>,</p>
    <p style='color:#555;'>Bạn nhận được lời mời ghép trận từ <strong style='color:#0EA86A;'>{tenUserA}</strong>!</p>
    <div style='background:#f8fffe;border:1px solid #1ed760;border-radius:12px;padding:20px;margin:20px 0;'>
      <table style='width:100%;font-size:.9rem;border-collapse:collapse;'>
        <tr><td style='color:#888;padding:5px 0;width:120px;'>👤 Đội mời</td><td style='color:#333;font-weight:700;'>{tenUserA}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>🏟 Sân của họ</td><td style='color:#333;'>{tenSanA}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>📅 Ngày</td><td style='color:#0EA86A;font-weight:700;'>{ngay}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>⏰ Khung giờ</td><td style='color:#0EA86A;font-weight:700;'>{gio}</td></tr>
      </table>
      <div style='margin-top:14px;padding:12px;background:#fff;border-radius:8px;border:1px solid #eee;'>
        <div style='font-size:.75rem;color:#888;margin-bottom:4px;'>💬 Lời nhắn:</div>
        <div style='color:#333;font-style:italic;'>""{loiNhan}""</div>
      </div>
    </div>
    <div style='text-align:center;margin-top:20px;'>
      <a href='https://localhost:7000/Booking/LoiMoiGhepTran'
         style='background:#0EA86A;color:#fff;text-decoration:none;padding:12px 28px;
                border-radius:999px;font-weight:700;font-size:.95rem;display:inline-block;'>
        Xem và phản hồi lời mời →
      </a>
    </div>
    <p style='color:#888;font-size:.8rem;margin-top:16px;text-align:center;'>
      Đăng nhập vào PitchHub để chấp nhận hoặc từ chối lời mời này.
    </p>
  </div>
  <div style='background:#f8f8f8;padding:14px;text-align:center;border-top:1px solid #eee;'>
    <p style='color:#aaa;font-size:.75rem;margin:0;'>© {DateTime.Now.Year} PitchHub.vn</p>
  </div>
</div>
</body></html>";
            await GuiEmailAsync(emailB, tenUserB,
                $"[PitchHub] ⚽ {tenUserA} mời bạn ghép trận — {ngay} {gio}", body);
        }

        // ══════════════════════════════════════════════════════════
        // 13. TỪ CHỐI GHÉP TRẬN → USER A
        // ══════════════════════════════════════════════════════════
        public async Task GuiEmailGhepTranTuChoi(
            string emailA, string tenUserA, string tenUserB)
        {
            var body = $@"
<!DOCTYPE html><html lang='vi'><head><meta charset='utf-8'></head>
<body style='font-family:Arial,sans-serif;background:#f4f4f4;margin:0;padding:20px;'>
<div style='max-width:560px;margin:0 auto;background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,.1);'>
  <div style='background:linear-gradient(135deg,#1a0a0a,#3a1a1a);padding:28px;text-align:center;'>
    <div style='font-size:1.8rem;font-weight:900;color:#fff;'>PITCH<span style='color:#ef4444;'>HUB</span>⚽</div>
    <div style='color:#ef4444;font-size:.85rem;letter-spacing:2px;margin-top:4px;'>LỜI MỜI BỊ TỪ CHỐI</div>
  </div>
  <div style='padding:28px;'>
    <p style='color:#333;'>Xin chào <strong>{tenUserA}</strong>,</p>
    <p style='color:#555;'><strong style='color:#ef4444;'>{tenUserB}</strong> đã từ chối lời mời ghép trận của bạn.</p>
    <p style='color:#777;font-size:.9rem;'>Đừng nản lòng! Bạn có thể tìm đối thủ khác trên <strong>PitchHub</strong> hoặc đăng bài tìm đối thủ tại trang Matchmaking.</p>
    <div style='text-align:center;margin-top:20px;'>
      <a href='https://localhost:7000/Matchmaking'
         style='background:#0EA86A;color:#fff;text-decoration:none;padding:12px 28px;
                border-radius:999px;font-weight:700;font-size:.95rem;display:inline-block;'>
        Tìm đối thủ khác →
      </a>
    </div>
  </div>
  <div style='background:#f8f8f8;padding:14px;text-align:center;border-top:1px solid #eee;'>
    <p style='color:#aaa;font-size:.75rem;margin:0;'>© {DateTime.Now.Year} PitchHub.vn</p>
  </div>
</div>
</body></html>";
            await GuiEmailAsync(emailA, tenUserA,
                $"[PitchHub] {tenUserB} đã từ chối lời mời ghép trận", body);
        }

        // ══════════════════════════════════════════════════════════
        // 14. GHÉP TRẬN HOÀN TẤT (sân A hoặc B) → 2 đội
        // ══════════════════════════════════════════════════════════
        public async Task GuiEmailGhepTranHoanTat(
            string emailA, string tenUserA,
            string emailB, string tenUserB,
            string tenSan, string ngay, string gio, string maXacNhan)
        {
            async Task GuiMot(string email, string ten)
            {
                var body = $@"
<!DOCTYPE html><html lang='vi'><head><meta charset='utf-8'></head>
<body style='font-family:Arial,sans-serif;background:#f4f4f4;margin:0;padding:20px;'>
<div style='max-width:560px;margin:0 auto;background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,.1);'>
  <div style='background:linear-gradient(135deg,#0f2027,#1a3a2a);padding:28px;text-align:center;'>
    <div style='font-size:1.8rem;font-weight:900;color:#fff;'>PITCH<span style='color:#1ed760;'>HUB</span>⚽</div>
    <div style='color:#1ed760;font-size:.85rem;letter-spacing:2px;margin-top:4px;'>GHÉP TRẬN THÀNH CÔNG! 🎉</div>
  </div>
  <div style='padding:28px;'>
    <p style='color:#333;'>Xin chào <strong>{ten}</strong>,</p>
    <p style='color:#555;'>Trận đấu ghép cặp của bạn đã được xác nhận!</p>
    <div style='background:#f8fffe;border:1px solid #1ed760;border-radius:12px;padding:20px;margin:20px 0;'>
      <table style='width:100%;font-size:.9rem;border-collapse:collapse;'>
        <tr><td style='color:#888;padding:5px 0;width:120px;'>🏟 Sân đấu</td><td style='color:#0EA86A;font-weight:700;'>{tenSan}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>📅 Ngày</td><td style='color:#0EA86A;font-weight:700;'>{ngay}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>⏰ Giờ đá</td><td style='color:#0EA86A;font-weight:700;'>{gio}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>🆚 Đối đầu</td><td style='color:#333;font-weight:700;'>{tenUserA} vs {tenUserB}</td></tr>
      </table>
      <div style='background:#0f1f14;border-radius:10px;padding:14px;text-align:center;margin-top:12px;'>
        <div style='color:#888;font-size:.72rem;letter-spacing:1.5px;'>MÃ CHECK-IN</div>
        <div style='font-family:monospace;font-size:1.4rem;font-weight:900;color:#1ed760;letter-spacing:3px;margin-top:4px;'>{maXacNhan}</div>
      </div>
    </div>
    <p style='color:#666;font-size:.85rem;text-align:center;'>Chúc các bạn có trận đấu sôi nổi! 🏆</p>
  </div>
  <div style='background:#f8f8f8;padding:14px;text-align:center;border-top:1px solid #eee;'>
    <p style='color:#aaa;font-size:.75rem;margin:0;'>© {DateTime.Now.Year} PitchHub.vn</p>
  </div>
</div>
</body></html>";
                await GuiEmailAsync(email, ten,
                    $"[PitchHub] 🎉 Ghép trận thành công — {tenSan} {ngay}", body);
            }
            await GuiMot(emailA, tenUserA);
            await GuiMot(emailB, tenUserB);
        }

        // ══════════════════════════════════════════════════════════
        // 15. GHÉP TRẬN SÂN MỚI → 2 đội
        // ══════════════════════════════════════════════════════════
        public async Task GuiEmailGhepTranSanMoi(
            string emailA, string tenUserA,
            string emailB, string tenUserB,
            string tenSanMoi, string ngay, string gio, string maXacNhan)
        {
            async Task GuiMot(string email, string ten)
            {
                var body = $@"
<!DOCTYPE html><html lang='vi'><head><meta charset='utf-8'></head>
<body style='font-family:Arial,sans-serif;background:#f4f4f4;margin:0;padding:20px;'>
<div style='max-width:560px;margin:0 auto;background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,.1);'>
  <div style='background:linear-gradient(135deg,#0f2027,#1a3a2a);padding:28px;text-align:center;'>
    <div style='font-size:1.8rem;font-weight:900;color:#fff;'>PITCH<span style='color:#1ed760;'>HUB</span>⚽</div>
    <div style='color:#1ed760;font-size:.85rem;letter-spacing:2px;margin-top:4px;'>SÂN MỚI ĐÃ ĐẶT! 🆕</div>
  </div>
  <div style='padding:28px;'>
    <p style='color:#333;'>Xin chào <strong>{ten}</strong>,</p>
    <p style='color:#555;'>Ghép trận thành công! Hai đội đã đồng thuận chuyển sang <strong style='color:#0EA86A;'>sân mới</strong>.</p>
    <div style='background:#f8fffe;border:1px solid #1ed760;border-radius:12px;padding:20px;margin:20px 0;'>
      <table style='width:100%;font-size:.9rem;border-collapse:collapse;'>
        <tr><td style='color:#888;padding:5px 0;width:120px;'>🏟 Sân mới</td><td style='color:#0EA86A;font-weight:700;'>{tenSanMoi}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>📅 Ngày</td><td style='color:#0EA86A;font-weight:700;'>{ngay}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>⏰ Giờ đá</td><td style='color:#0EA86A;font-weight:700;'>{gio}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>🆚 Đối đầu</td><td style='color:#333;font-weight:700;'>{tenUserA} vs {tenUserB}</td></tr>
      </table>
      <div style='background:#0f1f14;border-radius:10px;padding:14px;text-align:center;margin-top:12px;'>
        <div style='color:#888;font-size:.72rem;letter-spacing:1.5px;'>MÃ CHECK-IN</div>
        <div style='font-family:monospace;font-size:1.4rem;font-weight:900;color:#1ed760;letter-spacing:3px;margin-top:4px;'>{maXacNhan}</div>
      </div>
    </div>
    <p style='color:#888;font-size:.8rem;'>Đơn đặt sân cũ của bạn đã được hủy tự động và hoàn cọc theo quy định.</p>
    <p style='color:#666;font-size:.85rem;text-align:center;'>Chúc các bạn có trận đấu sôi nổi! 🏆</p>
  </div>
  <div style='background:#f8f8f8;padding:14px;text-align:center;border-top:1px solid #eee;'>
    <p style='color:#aaa;font-size:.75rem;margin:0;'>© {DateTime.Now.Year} PitchHub.vn</p>
  </div>
</div>
</body></html>";
                await GuiEmailAsync(email, ten,
                    $"[PitchHub] 🆕 Ghép trận — sân mới {tenSanMoi} {ngay}", body);
            }
            await GuiMot(emailA, tenUserA);
            await GuiMot(emailB, tenUserB);
        }

        // ══════════════════════════════════════════════════════════
        // 16. CÓ NGƯỜI TIẾP NHẬN CHUYỂN NHƯỢNG → USER A
        // ══════════════════════════════════════════════════════════
        public async Task GuiEmailCoNguoiTiepNhan(
            string emailA, string tenUserA, string tenUserB,
            string tenSan, string ngay, string gio)
        {
            var body = $@"
<!DOCTYPE html><html lang='vi'><head><meta charset='utf-8'></head>
<body style='font-family:Arial,sans-serif;background:#f4f4f4;margin:0;padding:20px;'>
<div style='max-width:560px;margin:0 auto;background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,.1);'>
  <div style='background:linear-gradient(135deg,#0f2027,#1a3a2a);padding:28px;text-align:center;'>
    <div style='font-size:1.8rem;font-weight:900;color:#fff;'>PITCH<span style='color:#1ed760;'>HUB</span>⚽</div>
    <div style='color:#1ed760;font-size:.85rem;letter-spacing:2px;margin-top:4px;'>CÓ NGƯỜI MUỐN TIẾP NHẬN ĐƠN 🔔</div>
  </div>
  <div style='padding:28px;'>
    <p style='color:#333;'>Xin chào <strong>{tenUserA}</strong>,</p>
    <p style='color:#555;'><strong style='color:#0EA86A;'>{tenUserB}</strong> muốn tiếp nhận chuyển nhượng đơn đặt sân của bạn!</p>
    <div style='background:#f8fffe;border:1px solid #1ed760;border-radius:12px;padding:20px;margin:20px 0;'>
      <table style='width:100%;font-size:.9rem;border-collapse:collapse;'>
        <tr><td style='color:#888;padding:5px 0;width:120px;'>🏟 Sân</td><td style='color:#333;font-weight:700;'>{tenSan}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>📅 Ngày</td><td style='color:#0EA86A;font-weight:700;'>{ngay}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>⏰ Giờ</td><td style='color:#0EA86A;font-weight:700;'>{gio}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>👤 Người nhận</td><td style='color:#333;font-weight:700;'>{tenUserB}</td></tr>
      </table>
    </div>
    <p style='color:#555;'>Hãy đăng nhập và xác nhận hoặc từ chối yêu cầu này.</p>
    <div style='text-align:center;margin-top:20px;'>
      <a href='https://localhost:7000/ChuyenNhuong/Index'
         style='background:#0EA86A;color:#fff;text-decoration:none;padding:12px 28px;
                border-radius:999px;font-weight:700;font-size:.95rem;display:inline-block;'>
        Xem và xác nhận →
      </a>
    </div>
  </div>
  <div style='background:#f8f8f8;padding:14px;text-align:center;border-top:1px solid #eee;'>
    <p style='color:#aaa;font-size:.75rem;margin:0;'>© {DateTime.Now.Year} PitchHub.vn</p>
  </div>
</div>
</body></html>";
            await GuiEmailAsync(emailA, tenUserA,
                $"[PitchHub] 🔔 {tenUserB} muốn tiếp nhận đơn sân {tenSan} — {ngay}", body);
        }

        // ══════════════════════════════════════════════════════════
        // 17. CHUYỂN NHƯỢNG HOÀN TẤT → CẢ A VÀ B
        // ══════════════════════════════════════════════════════════
        public async Task GuiEmailChuyenNhuongHoanTat(
            string emailA, string tenUserA,
            string emailB, string tenUserB,
            string tenSan, string ngay, string gio, string maXacNhan)
        {
            async Task GuiMot(string email, string ten, string vai)
            {
                var body = $@"
<!DOCTYPE html><html lang='vi'><head><meta charset='utf-8'></head>
<body style='font-family:Arial,sans-serif;background:#f4f4f4;margin:0;padding:20px;'>
<div style='max-width:560px;margin:0 auto;background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,.1);'>
  <div style='background:linear-gradient(135deg,#0f4c1e,#1a7a35);padding:28px;text-align:center;'>
    <div style='font-size:1.8rem;font-weight:900;color:#fff;'>PITCH<span style='color:#a8ffc4;'>HUB</span>⚽</div>
    <div style='color:#a8ffc4;font-size:.85rem;letter-spacing:2px;margin-top:4px;'>CHUYỂN NHƯỢNG HOÀN TẤT ✅</div>
  </div>
  <div style='padding:28px;'>
    <p style='color:#333;'>Xin chào <strong>{ten}</strong>,</p>
    <p style='color:#555;'>Chuyển nhượng đơn đặt sân đã được hoàn tất thành công! Bạn là <strong>{vai}</strong>.</p>
    <div style='background:#f0fff6;border:1px solid #0EA86A;border-radius:12px;padding:20px;margin:20px 0;'>
      <table style='width:100%;font-size:.9rem;border-collapse:collapse;'>
        <tr><td style='color:#888;padding:5px 0;width:130px;'>🏟 Sân</td><td style='color:#333;font-weight:700;'>{tenSan}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>📅 Ngày</td><td style='color:#0EA86A;font-weight:700;'>{ngay}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>⏰ Giờ</td><td style='color:#0EA86A;font-weight:700;'>{gio}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>📋 Mã xác nhận</td><td style='color:#333;font-weight:700;'>{maXacNhan}</td></tr>
      </table>
    </div>
    <p style='color:#888;font-size:.85rem;'>⚠️ Mọi giao dịch tài chính liên quan được thực hiện ngoài hệ thống theo thỏa thuận giữa hai bên.</p>
  </div>
  <div style='background:#f8f8f8;padding:14px;text-align:center;border-top:1px solid #eee;'>
    <p style='color:#aaa;font-size:.75rem;margin:0;'>© {DateTime.Now.Year} PitchHub.vn</p>
  </div>
</div>
</body></html>";
                await GuiEmailAsync(email, ten,
                    $"[PitchHub] ✅ Chuyển nhượng hoàn tất — {tenSan} {ngay}", body);
            }
            await GuiMot(emailA, tenUserA, "bên chuyển nhượng");
            await GuiMot(emailB, tenUserB, "bên tiếp nhận");
        }

        // ══════════════════════════════════════════════════════════
        // 18. CHUYỂN NHƯỢNG BỊ TỪ CHỐI → CÁ NHÂN
        // ══════════════════════════════════════════════════════════
        public async Task GuiEmailChuyenNhuongTuChoi(
            string email, string tenUser,
            string tenSan, string ngay, string gio, string lyDo)
        {
            var body = $@"
<!DOCTYPE html><html lang='vi'><head><meta charset='utf-8'></head>
<body style='font-family:Arial,sans-serif;background:#f4f4f4;margin:0;padding:20px;'>
<div style='max-width:560px;margin:0 auto;background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,.1);'>
  <div style='background:linear-gradient(135deg,#1a0a0a,#3a1a1a);padding:28px;text-align:center;'>
    <div style='font-size:1.8rem;font-weight:900;color:#fff;'>PITCH<span style='color:#ef4444;'>HUB</span>⚽</div>
    <div style='color:#ef4444;font-size:.85rem;letter-spacing:2px;margin-top:4px;'>CHUYỂN NHƯỢNG BỊ TỪ CHỐI</div>
  </div>
  <div style='padding:28px;'>
    <p style='color:#333;'>Xin chào <strong>{tenUser}</strong>,</p>
    <p style='color:#555;'>Yêu cầu chuyển nhượng đơn đặt sân dưới đây không được chấp thuận.</p>
    <div style='background:#fff5f5;border:1px solid #ef4444;border-radius:12px;padding:16px;margin:20px 0;'>
      <table style='width:100%;font-size:.9rem;border-collapse:collapse;'>
        <tr><td style='color:#888;padding:5px 0;width:100px;'>🏟 Sân</td><td style='color:#333;'>{tenSan}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>📅 Ngày</td><td style='color:#333;'>{ngay}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>⏰ Giờ</td><td style='color:#333;'>{gio}</td></tr>
      </table>
      <div style='margin-top:12px;padding-top:10px;border-top:1px solid #fecaca;'>
        <div style='color:#888;font-size:.8rem;margin-bottom:4px;'>Lý do:</div>
        <div style='color:#333;font-style:italic;'>{lyDo}</div>
      </div>
    </div>
  </div>
  <div style='background:#f8f8f8;padding:14px;text-align:center;border-top:1px solid #eee;'>
    <p style='color:#aaa;font-size:.75rem;margin:0;'>© {DateTime.Now.Year} PitchHub.vn</p>
  </div>
</div>
</body></html>";
            await GuiEmailAsync(email, tenUser,
                $"[PitchHub] Chuyển nhượng đơn sân {tenSan} không được chấp thuận", body);
        }

        // ══════════════════════════════════════════════════════════
        // 19. CHUYỂN NHƯỢNG CHỜ STAFF → STAFF
        // ══════════════════════════════════════════════════════════
        public async Task GuiEmailChuyenNhuongChoStaff(
            string emailStaff, string tenStaff,
            string tenUserA, string tenUserB,
            string tenSan, string ngay, string gio, decimal gia)
        {
            var giaHien = gia == 0 ? "Miễn phí" : $"{gia:N0} đ (thỏa thuận ngoài hệ thống)";
            var body = $@"
<!DOCTYPE html><html lang='vi'><head><meta charset='utf-8'></head>
<body style='font-family:Arial,sans-serif;background:#f4f4f4;margin:0;padding:20px;'>
<div style='max-width:560px;margin:0 auto;background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,.1);'>
  <div style='background:linear-gradient(135deg,#1e3a5f,#0f2540);padding:28px;text-align:center;'>
    <div style='font-size:1.8rem;font-weight:900;color:#fff;'>PITCH<span style='color:#60a5fa;'>HUB</span>⚽</div>
    <div style='color:#60a5fa;font-size:.85rem;letter-spacing:2px;margin-top:4px;'>YÊU CẦU KIỂM DUYỆT CHUYỂN NHƯỢNG</div>
  </div>
  <div style='padding:28px;'>
    <p style='color:#333;'>Xin chào <strong>{tenStaff}</strong>,</p>
    <p style='color:#555;'>Có yêu cầu chuyển nhượng đơn đặt sân cần kiểm duyệt.</p>
    <div style='background:#f0f7ff;border:1px solid #60a5fa;border-radius:12px;padding:20px;margin:20px 0;'>
      <table style='width:100%;font-size:.9rem;border-collapse:collapse;'>
        <tr><td style='color:#888;padding:5px 0;width:130px;'>👤 Bên chuyển</td><td style='color:#333;font-weight:700;'>{tenUserA}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>👤 Bên nhận</td><td style='color:#333;font-weight:700;'>{tenUserB}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>🏟 Sân</td><td style='color:#333;'>{tenSan}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>📅 Ngày</td><td style='color:#333;'>{ngay}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>⏰ Giờ</td><td style='color:#333;'>{gio}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>💰 Giá</td><td style='color:#333;'>{giaHien}</td></tr>
      </table>
    </div>
    <div style='text-align:center;margin-top:20px;'>
      <a href='https://localhost:7000/Staff/ChuyenNhuong'
         style='background:#3b82f6;color:#fff;text-decoration:none;padding:12px 28px;
                border-radius:999px;font-weight:700;font-size:.95rem;display:inline-block;'>
        Kiểm duyệt ngay →
      </a>
    </div>
  </div>
  <div style='background:#f8f8f8;padding:14px;text-align:center;border-top:1px solid #eee;'>
    <p style='color:#aaa;font-size:.75rem;margin:0;'>© {DateTime.Now.Year} PitchHub.vn</p>
  </div>
</div>
</body></html>";
            await GuiEmailAsync(emailStaff, tenStaff,
                $"[PitchHub] 📋 Kiểm duyệt chuyển nhượng: {tenSan} — {tenUserA} → {tenUserB}", body);
        }
    }
}