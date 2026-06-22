using Microsoft.AspNetCore.Mvc;
using QRCoder;

namespace Web_Stadium.Controllers
{
    public class QRController : Controller
    {
        private readonly IConfiguration _config;
        public QRController(IConfiguration config) => _config = config;

        // GET /QR/Code?text=ABC123&size=240
        // Sinh QR PNG cho mã xác nhận đơn — staff scan để check-in
        [HttpGet]
        public IActionResult Code(string text, int size = 240)
        {
            if (string.IsNullOrWhiteSpace(text))
                return BadRequest("Missing text");

            using var gen = new QRCodeGenerator();
            using var data = gen.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            var png = new PngByteQRCode(data).GetGraphic(Math.Max(2, size / 30));
            return File(png, "image/png");
        }

        // GET /QR/VietQR?amount=300000&info=DEMO0001
        // Redirect đến VietQR public API — khách quét bằng app ngân hàng để chuyển khoản
        [HttpGet]
        public IActionResult VietQR(decimal amount, string? info, string template = "compact2")
        {
            var bin  = _config["Payment:BankBIN"]        ?? "970407";
            var acc  = _config["Payment:AccountNumber"]  ?? "19038396495013";
            var name = _config["Payment:AccountName"]    ?? "DAO VIET TOAN";

            // img.vietqr.io public service — không cần API key
            var url = $"https://img.vietqr.io/image/{bin}-{acc}-{template}.png" +
                      $"?amount={amount:F0}" +
                      $"&addInfo={Uri.EscapeDataString(info ?? "")}" +
                      $"&accountName={Uri.EscapeDataString(name)}";

            return Redirect(url);
        }
    }
}
