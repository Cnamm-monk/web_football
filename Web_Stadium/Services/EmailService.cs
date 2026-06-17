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
            string tenSan, string loaiSan, string diaChi,
            string khungGio, string ngayThiDau,
            string soDienThoai, string maXacNhan,
            decimal giaThue, decimal tongDichVu,
            List<(string tenDv, int sl, decimal gia)> dichVus,
            decimal tongGoc,
            decimal tienGiamSan, string? tenVoucherSan,
            decimal tienGiamHeThong, string? tenVoucherHT,
            decimal tongSauGiam, decimal tienCoc, decimal conLai)
        {
            var mapsUrl = $"https://maps.google.com/?q={Uri.EscapeDataString(diaChi)}";
            var qrUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=200x200&data={maXacNhan}";

            // Build dịch vụ rows
            var dvRows = new System.Text.StringBuilder();
            foreach (var (tenDv, sl, gia) in dichVus)
            {
                dvRows.AppendLine($@"
        <tr>
          <td style='padding:5px 0;color:#ccc;'>{tenDv}</td>
          <td style='padding:5px 0;color:#ccc;text-align:center;'>{sl}</td>
          <td style='padding:5px 0;color:#ccc;text-align:right;'>{gia:N0}đ</td>
          <td style='padding:5px 0;color:#fff;text-align:right;font-weight:600;'>{sl * gia:N0}đ</td>
        </tr>");
            }

            var dvSection = dichVus.Count > 0 ? $@"
    <!-- Dịch vụ kèm -->
    <div style='background:#0f1f14;border-radius:12px;padding:18px;margin-bottom:16px;'>
      <div style='color:#1ed760;font-size:.75rem;letter-spacing:1.5px;font-weight:700;margin-bottom:12px;'>DỊCH VỤ KÈM</div>
      <table style='width:100%;border-collapse:collapse;font-size:.85rem;'>
        <thead>
          <tr style='border-bottom:1px solid rgba(255,255,255,.1);'>
            <th style='color:#888;font-weight:400;padding-bottom:8px;text-align:left;'>Dịch vụ</th>
            <th style='color:#888;font-weight:400;padding-bottom:8px;text-align:center;'>SL</th>
            <th style='color:#888;font-weight:400;padding-bottom:8px;text-align:right;'>Đơn giá</th>
            <th style='color:#888;font-weight:400;padding-bottom:8px;text-align:right;'>Thành tiền</th>
          </tr>
        </thead>
        <tbody>{dvRows}</tbody>
      </table>
    </div>" : "";

            // Build voucher rows
            var voucherSanRow = tienGiamSan > 0 ? $@"
        <tr>
          <td style='padding:6px 0;color:#aaa;'>🏟 Voucher sân ({tenVoucherSan})</td>
          <td style='padding:6px 0;color:#ef4444;text-align:right;font-weight:600;'>-{tienGiamSan:N0}đ</td>
        </tr>" : "";

            var voucherHTRow = tienGiamHeThong > 0 ? $@"
        <tr>
          <td style='padding:6px 0;color:#aaa;'>🎫 Voucher hệ thống ({tenVoucherHT})</td>
          <td style='padding:6px 0;color:#ef4444;text-align:right;font-weight:600;'>-{tienGiamHeThong:N0}đ</td>
        </tr>" : "";

            var body = $@"
<!DOCTYPE html>
<html lang='vi'>
<head><meta charset='utf-8'></head>
<body style='font-family:Arial,sans-serif;background:#0a0f0a;margin:0;padding:20px;'>
<div style='max-width:600px;margin:0 auto;background:#111;border-radius:16px;overflow:hidden;
            box-shadow:0 8px 32px rgba(0,0,0,.5);border:1px solid rgba(30,215,96,.15);'>

  <!-- Header -->
  <div style='background:linear-gradient(135deg,#0f2027,#1a3a2a);padding:36px 28px;text-align:center;'>
    <div style='font-size:2.2rem;font-weight:900;color:#fff;letter-spacing:-1px;'>
      PITCH<span style='color:#1ed760;'>HUB</span>⚽
    </div>
    <div style='color:#1ed760;font-size:.85rem;letter-spacing:3px;margin-top:6px;text-transform:uppercase;'>
      Đặt sân thành công
    </div>
  </div>

  <!-- Body -->
  <div style='padding:28px;'>
    <p style='color:#ccc;font-size:1rem;margin-bottom:24px;line-height:1.6;'>
      Xin chào <strong style='color:#fff;'>{toName}</strong>,<br>
      Đơn đặt sân của bạn đã được Owner xác nhận. Hẹn gặp bạn trên sân! ⚽
    </p>

    <!-- Thông tin booking -->
    <div style='background:#1a2a1a;border:1px solid rgba(30,215,96,.25);border-radius:12px;padding:20px;margin-bottom:20px;'>
      <div style='color:#1ed760;font-size:.75rem;letter-spacing:1.5px;font-weight:700;margin-bottom:14px;'>THÔNG TIN ĐẶT SÂN</div>
      <table style='width:100%;border-collapse:collapse;font-size:.9rem;'>
        <tr><td style='color:#888;padding:5px 0;width:140px;'>🏟 Sân</td>
            <td style='color:#fff;font-weight:700;'>{tenSan} ({loaiSan})</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>📍 Địa chỉ</td>
            <td style='color:#ddd;'>{diaChi}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>📅 Ngày</td>
            <td style='color:#fff;font-weight:600;'>{ngayThiDau}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>⏰ Giờ</td>
            <td style='color:#fff;font-weight:600;'>{khungGio}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>👤 Người đặt</td>
            <td style='color:#ddd;'>{toName}</td></tr>
        <tr><td style='color:#888;padding:5px 0;'>📞 SĐT</td>
            <td style='color:#ddd;'>{(string.IsNullOrEmpty(soDienThoai) ? "—" : soDienThoai)}</td></tr>
      </table>
    </div>

    {dvSection}

    <!-- Bảng tính tiền -->
    <div style='background:#0f1f14;border-radius:12px;padding:20px;margin-bottom:20px;'>
      <div style='color:#1ed760;font-size:.75rem;letter-spacing:1.5px;font-weight:700;margin-bottom:14px;'>CHI TIẾT THANH TOÁN</div>
      <table style='width:100%;border-collapse:collapse;font-size:.9rem;'>
        <tr>
          <td style='padding:6px 0;color:#aaa;'>Giá thuê sân</td>
          <td style='padding:6px 0;color:#fff;text-align:right;'>{giaThue:N0}đ</td>
        </tr>
        {(tongDichVu > 0 ? $@"<tr>
          <td style='padding:6px 0;color:#aaa;'>Dịch vụ kèm</td>
          <td style='padding:6px 0;color:#fff;text-align:right;'>{tongDichVu:N0}đ</td>
        </tr>" : "")}
        <tr style='border-top:1px solid rgba(255,255,255,.08);'>
          <td style='padding:8px 0;color:#ccc;font-weight:600;'>Tổng gốc</td>
          <td style='padding:8px 0;color:#fff;text-align:right;font-weight:600;'>{tongGoc:N0}đ</td>
        </tr>
        {voucherSanRow}
        {voucherHTRow}
        {(tienGiamSan + tienGiamHeThong > 0 ? $@"<tr style='border-top:1px solid rgba(30,215,96,.2);'>
          <td style='padding:8px 0;color:#1ed760;font-weight:700;'>Tổng sau giảm</td>
          <td style='padding:8px 0;color:#1ed760;text-align:right;font-weight:700;'>{tongSauGiam:N0}đ</td>
        </tr>" : "")}
        <tr style='border-top:1px solid rgba(255,255,255,.08);'>
          <td style='padding:8px 0;color:#aaa;'>Tiền cọc đặt trước</td>
          <td style='padding:8px 0;color:#1ed760;text-align:right;font-weight:700;'>{tienCoc:N0}đ</td>
        </tr>
        <tr>
          <td style='padding:6px 0;color:#aaa;font-size:.85rem;'>Còn lại khi đến sân</td>
          <td style='padding:6px 0;color:#ccc;text-align:right;font-size:.85rem;'>{conLai:N0}đ</td>
        </tr>
      </table>
    </div>

    <!-- Mã xác nhận -->
    <div style='background:#0f1f14;border:1px solid rgba(30,215,96,.3);border-radius:12px;padding:20px;text-align:center;margin-bottom:20px;'>
      <div style='color:#888;font-size:.75rem;letter-spacing:2px;margin-bottom:10px;text-transform:uppercase;'>Mã xác nhận check-in</div>
      <div style='font-family:monospace;font-size:1.8rem;font-weight:900;color:#1ed760;letter-spacing:4px;'>
        {maXacNhan}
      </div>
      <div style='color:#555;font-size:.78rem;margin-top:8px;'>Đưa mã này cho Staff khi đến sân</div>
    </div>

    <!-- QR Code -->
    <div style='text-align:center;margin-bottom:20px;'>
      <div style='color:#666;font-size:.75rem;letter-spacing:1px;margin-bottom:12px;text-transform:uppercase;'>Hoặc quét QR Code</div>
      <img src='{qrUrl}' width='160' height='160'
           style='border-radius:12px;border:3px solid #1ed760;' alt='QR Code' />
    </div>

    <!-- Google Maps -->
    <div style='text-align:center;margin-bottom:24px;'>
      <a href='{mapsUrl}' target='_blank'
         style='display:inline-block;background:#1ed760;color:#000;font-weight:700;
                padding:12px 32px;border-radius:10px;text-decoration:none;font-size:.9rem;letter-spacing:.5px;'>
        📍 Chỉ đường đến sân
      </a>
    </div>

    <p style='color:#555;font-size:.8rem;text-align:center;'>
      Bạn sẽ nhận email nhắc lịch trước 24 giờ và 1 giờ trước trận.
    </p>
  </div>

  <!-- Footer -->
  <div style='background:#0a0a0a;padding:16px;text-align:center;border-top:1px solid rgba(255,255,255,.05);'>
    <p style='color:#444;font-size:.75rem;margin:0;'>
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
    }
}