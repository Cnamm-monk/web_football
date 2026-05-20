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
        public async Task<IActionResult> Create(int khungGioId, DateTime? ngay)
        {
            // Xác định ngày hợp lệ: nếu không có hoặc nhỏ hơn hôm nay thì lấy ngày mai
            var ngayValid = ngay ?? DateTime.Now.Date.AddDays(1);
            if (ngayValid < DateTime.Now.Date)
                ngayValid = DateTime.Now.Date.AddDays(1);

            // Lấy khung giờ kèm sân và dịch vụ
            var khungGio = await _context.KhungGios
                .Include(k => k.SanBong)
                    .ThenInclude(s => s.DichVus)
                        .ThenInclude(d => d.DanhMucDichVu)
                .FirstOrDefaultAsync(k => k.Id == khungGioId);

            if (khungGio == null) return NotFound();

            // Kiểm tra và giải phóng giữ chỗ hết hạn
            if (khungGio.TrangThai == "DangGiu" && khungGio.ThoiGianHetGiuCho < DateTime.Now)
            {
                khungGio.TrangThai = "Trong";
                khungGio.ThoiGianHetGiuCho = null;
                await _khungGioRepo.UpdateAsync(khungGio);
                await _hub.Clients.Group($"san_{khungGio.SanBongId}")
                    .SendAsync("CapNhatKhungGio", new { khungGioId = khungGio.Id, trangThai = "Trong" });
            }

            // Nếu khung giờ đã bị đặt
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

            var tyLeCoc = khungGio.SanBong?.TyLeCoc ?? 0.30m;
            var dichVus = khungGio.SanBong?.DichVus
                .Where(d => d.IsActive && d.TonKho > 0)
                .ToList() ?? new();

            ViewBag.KhungGio = khungGio;
            ViewBag.Ngay = ngayValid;
            ViewBag.TyLeCoc = tyLeCoc;
            ViewBag.TienCoc = khungGio.Gia * tyLeCoc;
            ViewBag.DichVus = dichVus;

            return View();
        }


        // ══════════════════════════════════════════════════════════
        // POST /Booking/Create
        // ══════════════════════════════════════════════════════════
        [HttpPost]
        [YeuCauDangNhap]
        public async Task<IActionResult> Create(
     int khungGioId,
     string ngayThiDau,
     List<int>? dichVuIds,
     List<int>? soLuongs)
        {
            // Lấy khung giờ để biết SanBongId (dùng cho redirect nếu lỗi)
            var khungGio = await _context.KhungGios
                .Include(k => k.SanBong)
                .FirstOrDefaultAsync(k => k.Id == khungGioId);
            if (khungGio == null) return NotFound();

            // Parse ngày thi đấu an toàn (định dạng yyyy-MM-dd)
            if (!DateTime.TryParseExact(ngayThiDau, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var ngay))
            {
                TempData["Error"] = "Ngày thi đấu không hợp lệ. Vui lòng chọn lại ngày.";
                return RedirectToAction("Details", "Venues", new { id = khungGio.SanBongId });
            }

            // Không cho đặt sân trong quá khứ
            if (ngay < DateTime.Now.Date)
            {
                TempData["Error"] = "Không thể đặt sân cho ngày trong quá khứ.";
                return RedirectToAction("Details", "Venues", new { id = khungGio.SanBongId });
            }

            // Kiểm tra lại khung giờ (tránh trường hợp giữ chỗ hết hạn)
            var khungGioMoi = await _context.KhungGios.FindAsync(khungGioId);
            if (khungGioMoi == null || khungGioMoi.TrangThai == "DaDat")
            {
                TempData["Error"] = "Khung giờ đã bị người khác đặt mất. Vui lòng chọn khung khác.";
                return RedirectToAction("Details", "Venues", new { id = khungGio.SanBongId });
            }

            var userId = TokenHelper.LayUserId(Request, _config);
            var tyLeCoc = khungGio.SanBong?.TyLeCoc ?? 0.30m;
            var tienCoc = khungGio.Gia * tyLeCoc;
            var maDatSan = Guid.NewGuid().ToString()[..8].ToUpper();

            var datSan = new DatSan
            {
                UserId = userId!.Value,
                KhungGioId = khungGioId,
                NgayThiDau = ngay,
                TienCoc = tienCoc,
                MaXacNhan = maDatSan,
                TrangThai = "DaXacNhan",   // Giả sử đã thanh toán cọc
                ThoiGianTao = DateTime.Now
            };
            await _datSanRepo.AddAsync(datSan);

            // Xử lý dịch vụ kèm theo, trừ tồn kho ngay khi đặt
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
                        SoLuong = sl
                    });
                    dv.TonKho -= sl;
                }
                await _context.SaveChangesAsync();
            }

            // Cập nhật trạng thái khung giờ thành "Đã đặt"
            khungGioMoi.TrangThai = "DaDat";
            khungGioMoi.ThoiGianHetGiuCho = null;
            await _khungGioRepo.UpdateAsync(khungGioMoi);
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
            ViewBag.TrangThai = trangThai;

            // Lay cac DatSanId da co tin matchmaking dang tim
            var datSanIds = list.Select(d => d.Id).ToList();
            var daTim = await _context.Matchmakings
                .Where(m => m.TrangThai == "DangTim" && datSanIds.Contains(m.DatSanId))
                .ToListAsync();

            ViewBag.DaTim = daTim.Select(m => m.DatSanId).ToHashSet();

            // Map DatSanId -> MatchmakingId de nut Huy tin biet id nao can huy
            var mmIdMap = daTim.ToDictionary(m => m.DatSanId, m => m.Id);
            ViewBag.MmIdMap = mmIdMap;

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
                UserId = userId!.Value,
                LyDo = lyDo,
                TrangThai = "ChoXuLy",
                NgayGui = DateTime.Now
            });
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã gửi khiếu nại! Admin sẽ xem xét và phản hồi sớm nhất.";
            return RedirectToAction("MyBookings");
        }
    }
}