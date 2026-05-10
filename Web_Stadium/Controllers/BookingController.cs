using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Web_Stadium.EFCore;
using Web_Stadium.Filters;
using Web_Stadium.Hubs;

namespace Web_Stadium.Controllers
{
    public class BookingController : Controller
    {
        private readonly SanBongContext _context;
        private readonly IRepository<DatSan> _datSanRepo;
        private readonly IRepository<KhungGio> _khungGioRepo;
        private readonly IConfiguration _config;
        private readonly IHubContext<SanBongHub> _hub;

        public BookingController(
            SanBongContext context,
            IRepository<DatSan> datSanRepo,
            IRepository<KhungGio> khungGioRepo,
            IConfiguration config,
            IHubContext<SanBongHub> hub)
        {
            _context = context;
            _datSanRepo = datSanRepo;
            _khungGioRepo = khungGioRepo;
            _config = config;
            _hub = hub;
        }

        // ══════════════════════════════════════════════════════════
        // GET /Booking/Create?khungGioId=1&ngay=2024-04-15
        // ══════════════════════════════════════════════════════════
        [YeuCauDangNhap]
        public async Task<IActionResult> Create(int khungGioId, DateTime ngay)
        {
            var userId = TokenHelper.LayUserId(Request, _config);

            var khungGio = await _context.KhungGios
                .Include(k => k.SanBong)
                    .ThenInclude(s => s.DichVus)
                        .ThenInclude(d => d.DanhMucDichVu)
                .FirstOrDefaultAsync(k => k.Id == khungGioId);

            if (khungGio == null) return NotFound();

            // Kiểm tra hết hạn giữ chỗ
            if (khungGio.TrangThai == "DangGiu"
                && khungGio.ThoiGianHetGiuCho < DateTime.Now)
            {
                khungGio.TrangThai = "Trong";
                khungGio.ThoiGianHetGiuCho = null;
                await _khungGioRepo.UpdateAsync(khungGio);
                await _hub.Clients.Group($"san_{khungGio.SanBongId}")
                    .SendAsync("CapNhatKhungGio", new { khungGioId = khungGio.Id, trangThai = "Trong" });
            }

            if (khungGio.TrangThai == "DaDat")
            {
                TempData["Error"] = "Khung giờ này đã bị đặt!";
                return RedirectToAction("Details", "Venues", new { id = khungGio.SanBongId });
            }

            // Giữ chỗ 10 phút
            khungGio.TrangThai = "DangGiu";
            khungGio.ThoiGianHetGiuCho = DateTime.Now.AddMinutes(10);
            await _khungGioRepo.UpdateAsync(khungGio);
            await _hub.Clients.Group($"san_{khungGio.SanBongId}")
                .SendAsync("CapNhatKhungGio", new
                {
                    khungGioId = khungGio.Id,
                    trangThai = "DangGiu",
                    hetHan = khungGio.ThoiGianHetGiuCho
                });

            // FIX 1: Tính tiền cọc theo TyLeCoc của sân (không cứng 30%)
            var tyLeCoc = khungGio.SanBong?.TyLeCoc ?? 0.30m;

            // FIX 2: Lấy dịch vụ của sân cụ thể (còn tồn kho)
            var dichVus = khungGio.SanBong?.DichVus
                .Where(d => d.IsActive && d.TonKho > 0)
                .ToList() ?? new();

            ViewBag.KhungGio = khungGio;
            ViewBag.Ngay     = ngay;
            ViewBag.TyLeCoc  = tyLeCoc;
            ViewBag.TienCoc  = khungGio.Gia * tyLeCoc;
            ViewBag.DichVus  = dichVus;

            return View();
        }

        // ══════════════════════════════════════════════════════════
        // POST /Booking/Create
        // ══════════════════════════════════════════════════════════
        [HttpPost]
        [YeuCauDangNhap]
        public async Task<IActionResult> Create(
            int khungGioId,
            DateTime ngayThiDau,
            List<int>? dichVuIds,
            List<int>? soLuongs)
        {
            var userId = TokenHelper.LayUserId(Request, _config);
            var khungGio = await _context.KhungGios
                .Include(k => k.SanBong)
                .FirstOrDefaultAsync(k => k.Id == khungGioId);
            if (khungGio == null) return NotFound();

            // FIX 1: Tính cọc theo TyLeCoc của sân
            var tyLeCoc = khungGio.SanBong?.TyLeCoc ?? 0.30m;
            var tienCoc = khungGio.Gia * tyLeCoc;

            var maDatSan = Guid.NewGuid().ToString()[..8].ToUpper();

            var datSan = new DatSan
            {
                UserId      = userId!.Value,
                KhungGioId  = khungGioId,
                NgayThiDau  = ngayThiDau,
                TienCoc     = tienCoc,
                MaXacNhan   = maDatSan,
                TrangThai   = "DaXacNhan",   // FIX: đã xác nhận (giả định đã thanh toán)
                ThoiGianTao = DateTime.Now
            };
            await _datSanRepo.AddAsync(datSan);

            // FIX 2: Thêm dịch vụ + trừ tồn kho ngay khi đặt
            if (dichVuIds != null)
            {
                for (int i = 0; i < dichVuIds.Count; i++)
                {
                    var sl = (soLuongs != null && i < soLuongs.Count) ? soLuongs[i] : 1;
                    if (sl <= 0) continue;

                    var dv = await _context.DichVus.FindAsync(dichVuIds[i]);
                    if (dv == null || dv.TonKho < sl) continue;

                    _context.DatSanDichVus.Add(new DatSanDichVu
                    {
                        DatSanId = datSan.Id,
                        DichVuId = dichVuIds[i],
                        SoLuong  = sl
                    });

                    // Trừ tồn kho ngay khi đặt trước
                    dv.TonKho -= sl;
                }
                await _context.SaveChangesAsync();
            }

            // Khoá slot
            khungGio.TrangThai = "DaDat";
            khungGio.ThoiGianHetGiuCho = null;
            await _khungGioRepo.UpdateAsync(khungGio);
            await _hub.Clients.Group($"san_{khungGio.SanBongId}")
                .SendAsync("CapNhatKhungGio", new { khungGioId = khungGio.Id, trangThai = "DaDat" });

            TempData["Success"] = $"Đặt sân thành công! Mã xác nhận: {maDatSan}";
            return RedirectToAction("MyBookings");
        }

        // ══════════════════════════════════════════════════════════
        // GET /Booking/MyBookings
        // ══════════════════════════════════════════════════════════
        [YeuCauDangNhap]
        public async Task<IActionResult> MyBookings(string? trangThai)
        {
            var userId = TokenHelper.LayUserId(Request, _config);

            var query = _context.DatSans
                .Include(d => d.KhungGio).ThenInclude(k => k.SanBong)
                .Include(d => d.DatSanDichVus).ThenInclude(dv => dv.DichVu)
                .Where(d => d.UserId == userId);

            if (!string.IsNullOrEmpty(trangThai))
                query = query.Where(d => d.TrangThai == trangThai);

            var list = await query
                .OrderByDescending(d => d.ThoiGianTao)
                .ToListAsync();

            // Kiểm tra đơn nào đã đánh giá
            var daDanhGiaIds = await _context.DanhGias
                .Where(dg => dg.UserId == userId)
                .Select(dg => dg.DatSanId)
                .ToListAsync();

            ViewBag.DaDanhGiaIds = daDanhGiaIds;
            ViewBag.TrangThai    = trangThai;

            return View(list);
        }

        // ══════════════════════════════════════════════════════════
        // POST /Booking/Huy — Huỷ đơn (chỉ ChoDuyet hoặc DaXacNhan)
        // ══════════════════════════════════════════════════════════
        [HttpPost]
        [YeuCauDangNhap]
        public async Task<IActionResult> Huy(int id)
        {
            var userId = TokenHelper.LayUserId(Request, _config);
            var datSan = await _context.DatSans
                .Include(d => d.KhungGio)
                .Include(d => d.DatSanDichVus).ThenInclude(dv => dv.DichVu)
                .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);

            if (datSan == null) return NotFound();

            if (datSan.TrangThai == "DangSuDung" || datSan.TrangThai == "HoanThanh")
            {
                TempData["Error"] = "Không thể huỷ đơn đang diễn ra hoặc đã hoàn thành!";
                return RedirectToAction("MyBookings");
            }

            datSan.TrangThai = "DaHuy";
            datSan.KhungGio.TrangThai = "Trong";

            // Hoàn tồn kho dịch vụ đặt trước khi hủy
            foreach (var dv in datSan.DatSanDichVus)
            {
                if (dv.DichVu != null)
                    dv.DichVu.TonKho += dv.SoLuong;
            }

            await _context.SaveChangesAsync();
            await _hub.Clients.Group($"san_{datSan.KhungGio.SanBongId}")
                .SendAsync("CapNhatKhungGio", new { khungGioId = datSan.KhungGioId, trangThai = "Trong" });

            TempData["Success"] = "Đã huỷ đặt sân thành công!";
            return RedirectToAction("MyBookings");
        }

        // ══════════════════════════════════════════════════════════
        // POST /Booking/GuiKhieuNai — User gửi khiếu nại
        // ══════════════════════════════════════════════════════════
        [HttpPost]
        [YeuCauDangNhap]
        public async Task<IActionResult> GuiKhieuNai(int datSanId, string lyDo)
        {
            var userId = TokenHelper.LayUserId(Request, _config);

            var don = await _context.DatSans
                .FirstOrDefaultAsync(d => d.Id == datSanId && d.UserId == userId);
            if (don == null) return NotFound();

            // Kiểm tra đã có khiếu nại chưa
            var daCoKN = await _context.KhieuNais
                .AnyAsync(k => k.DatSanId == datSanId && k.UserId == userId.Value);
            if (daCoKN)
            {
                TempData["Error"] = "Bạn đã gửi khiếu nại cho đơn này rồi!";
                return RedirectToAction("MyBookings");
            }

            _context.KhieuNais.Add(new KhieuNai
            {
                DatSanId = datSanId,
                UserId   = userId!.Value,
                LyDo     = lyDo,
                TrangThai = "ChoXuLy",
                NgayGui   = DateTime.Now
            });
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã gửi khiếu nại! Admin sẽ xem xét và phản hồi sớm nhất.";
            return RedirectToAction("MyBookings");
        }
    }
}