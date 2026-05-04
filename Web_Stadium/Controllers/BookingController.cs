using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Web_Stadium.End;
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

        // GET /Booking/Create?khungGioId=1&ngay=2024-04-15
        [YeuCauDangNhap]
        public async Task<IActionResult> Create(
            int khungGioId, DateTime ngay)
        {
            var userId = TokenHelper.LayUserId(Request, _config);

            var khungGio = await _context.KhungGios
                .Include(k => k.SanBong)
                .FirstOrDefaultAsync(k => k.Id == khungGioId);

            if (khungGio == null) return NotFound();

            // Kiểm tra hết hạn giữ chỗ
            if (khungGio.TrangThai == "DangGiu"
                && khungGio.ThoiGianHetGiuCho.HasValue
                && khungGio.ThoiGianHetGiuCho < DateTime.Now)
            {
                khungGio.TrangThai = "Trong";
                khungGio.ThoiGianHetGiuCho = null;
                await _khungGioRepo.UpdateAsync(khungGio);

                // Real-time: thông báo sân về trống
                await _hub.Clients
                    .Group($"san_{khungGio.SanBongId}")
                    .SendAsync("CapNhatKhungGio", new
                    {
                        khungGioId = khungGio.Id,
                        trangThai = "Trong"
                    });
            }

            if (khungGio.TrangThai == "DaDat")
            {
                TempData["Error"] = "Khung giờ này đã bị đặt!";
                return RedirectToAction("Details", "Venues",
                    new { id = khungGio.SanBongId });
            }

            // Giữ chỗ 10 phút
            khungGio.TrangThai = "DangGiu";
            khungGio.ThoiGianHetGiuCho = DateTime.Now.AddMinutes(10);
            await _khungGioRepo.UpdateAsync(khungGio);

            // Real-time: thông báo đang giữ chỗ
            await _hub.Clients
                .Group($"san_{khungGio.SanBongId}")
                .SendAsync("CapNhatKhungGio", new
                {
                    khungGioId = khungGio.Id,
                    trangThai = "DangGiu",
                    hetHan = khungGio.ThoiGianHetGiuCho
                });

            // Lấy danh sách dịch vụ
            var dichVus = await _context.DichVus.ToListAsync();

            ViewBag.KhungGio = khungGio;
            ViewBag.Ngay = ngay;
            ViewBag.TienCoc = khungGio.Gia * 0.3m;
            ViewBag.DichVus = dichVus;
            return View();
        }

        // POST /Booking/Create
        [HttpPost]
        [YeuCauDangNhap]
        public async Task<IActionResult> Create(
            int khungGioId,
            DateTime ngayThiDau,
            List<int>? dichVuIds,
            List<int>? soLuongs)
        {
            var userId = TokenHelper.LayUserId(Request, _config);
            var khungGio = await _khungGioRepo
                .GetByIdAsync(khungGioId);
            if (khungGio == null) return NotFound();

            var maDatSan = Guid.NewGuid()
                .ToString()[..8].ToUpper();

            var datSan = new DatSan
            {
                UserId = userId!.Value,
                KhungGioId = khungGioId,
                NgayThiDau = ngayThiDau,
                TienCoc = khungGio.Gia * 0.3m,
                MaXacNhan = maDatSan,
                TrangThai = "ChoDuyet",
                ThoiGianTao = DateTime.Now
            };
            await _datSanRepo.AddAsync(datSan);

            // Thêm dịch vụ kèm
            if (dichVuIds != null)
            {
                for (int i = 0; i < dichVuIds.Count; i++)
                {
                    var sl = (soLuongs != null && i < soLuongs.Count)
                        ? soLuongs[i] : 1;
                    if (sl > 0)
                    {
                        _context.DatSan_DichVus.Add(new DatSan_DichVu
                        {
                            DatSanId = datSan.Id,
                            DichVuId = dichVuIds[i],
                            SoLuong = sl
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }

            // Cập nhật trạng thái khung giờ → DaDat
            khungGio.TrangThai = "DaDat";
            khungGio.ThoiGianHetGiuCho = null;
            await _khungGioRepo.UpdateAsync(khungGio);

            // Real-time: push cho tất cả người đang xem sân này
            await _hub.Clients
                .Group($"san_{khungGio.SanBongId}")
                .SendAsync("CapNhatKhungGio", new
                {
                    khungGioId = khungGio.Id,
                    trangThai = "DaDat"
                });

            TempData["Success"] =
                $"Đặt sân thành công! Mã xác nhận: {maDatSan}";
            return RedirectToAction("MyBookings");
        }

        // GET /Booking/MyBookings
        [YeuCauDangNhap]
        public async Task<IActionResult> MyBookings(string? trangThai)
        {
            //Bo sung
            var userId = TokenHelper.LayUserId(Request, _config);
            
            var query = _context.DatSans.Where(d => d.UserId == userId);
            if (!string.IsNullOrEmpty(trangThai))
                query = query.Where(d => d.TrangThai == trangThai);
            
            var list = await _context.DatSans
                .Where(d => d.UserId == userId)
                .Include(d => d.KhungGio)
                    .ThenInclude(k => k.SanBong)
                .Include(d => d.DichVuKemTheo)
                    .ThenInclude(dv => dv.DichVu)
                .OrderByDescending(d => d.ThoiGianTao)
                .ToListAsync();
            return View(list);
        }

        // POST /Booking/Huy
        [HttpPost]
        [YeuCauDangNhap]
        public async Task<IActionResult> Huy(int id)
        {
            var userId = TokenHelper.LayUserId(Request, _config);
            var datSan = await _context.DatSans
                .Include(d => d.KhungGio)
                .FirstOrDefaultAsync(d => d.Id == id
                    && d.UserId == userId);

            if (datSan == null) return NotFound();

            datSan.TrangThai = "DaHuy";
            datSan.KhungGio.TrangThai = "Trong";
            await _context.SaveChangesAsync();

            // Real-time: sân về trống
            await _hub.Clients
                .Group($"san_{datSan.KhungGio.SanBongId}")
                .SendAsync("CapNhatKhungGio", new
                {
                    khungGioId = datSan.KhungGioId,
                    trangThai = "Trong"
                });

            TempData["Success"] = "Đã hủy đặt sân thành công!";
            return RedirectToAction("MyBookings");
        }
    }
}