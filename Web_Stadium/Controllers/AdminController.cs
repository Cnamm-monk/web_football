using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_Stadium.End;
using Web_Stadium.Filters;

namespace Web_Stadium.Controllers
{
    [YeuCauDangNhap("Admin")]
    public class AdminController : Controller
    {
        private readonly SanBongContext _context;

        public AdminController(SanBongContext context)
        {
            _context = context;
        }

        // GET /Admin — Dashboard tổng quan
        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var thangNay = new DateTime(now.Year, now.Month, 1);

            ViewBag.TongSan = await _context.SanBongs.CountAsync();
            ViewBag.TongUser = await _context.Users.CountAsync();
            ViewBag.TongDatSan = await _context.DatSans.CountAsync();
            ViewBag.ChoPheDuyet = await _context.SanBongs
                .CountAsync(s => s.TrangThaiDuyet == "ChoDuyet");

            // Doanh thu tháng này (tiền cọc đã xác nhận)
            ViewBag.DoanhThuThang = await _context.DatSans
                .Where(d => d.ThoiGianTao >= thangNay
                         && d.TrangThai == "DaXacNhan")
                .SumAsync(d => (decimal?)d.TienCoc) ?? 0;

            // Doanh thu 6 tháng gần nhất
            var doanhThu6Thang = new List<object>();
            for (int i = 5; i >= 0; i--)
            {
                var thang = now.AddMonths(-i);
                var batDau = new DateTime(thang.Year, thang.Month, 1);
                var ketThuc = batDau.AddMonths(1);
                var dt = await _context.DatSans
                    .Where(d => d.ThoiGianTao >= batDau
                             && d.ThoiGianTao < ketThuc
                             && d.TrangThai == "DaXacNhan")
                    .SumAsync(d => (decimal?)d.TienCoc) ?? 0;
                doanhThu6Thang.Add(new
                {
                    thang = thang.ToString("MM/yyyy"),
                    dt = (double)dt
                });
            }
            ViewBag.DoanhThu6Thang = doanhThu6Thang;

            // Top 5 sân được đặt nhiều nhất
            ViewBag.TopSan = await _context.DatSans
                .Include(d => d.KhungGio)
                    .ThenInclude(k => k.SanBong)
                .GroupBy(d => d.KhungGio.SanBong.TenSan)
                .Select(g => new { Ten = g.Key, SoLuot = g.Count() })
                .OrderByDescending(x => x.SoLuot)
                .Take(5)
                .ToListAsync();

            // Trạng thái đặt sân theo loại
            ViewBag.TheoTrangThai = await _context.DatSans
                .GroupBy(d => d.TrangThai)
                .Select(g => new { TrangThai = g.Key, SoLuong = g.Count() })
                .ToListAsync();

            return View();
        }

        // GET /Admin/DuyetSan
        public async Task<IActionResult> DuyetSan()
        {
            var list = await _context.SanBongs
                .Include(s => s.Owner)
                .Where(s => s.TrangThaiDuyet == "ChoDuyet")
                .OrderBy(s => s.Id)
                .ToListAsync();
            return View(list);
        }

        // POST /Admin/PheDuyet
        [HttpPost]
        public async Task<IActionResult> PheDuyet(
            int id, string trangThai)
        {
            var san = await _context.SanBongs.FindAsync(id);
            if (san == null) return NotFound();
            san.TrangThaiDuyet = trangThai;
            await _context.SaveChangesAsync();
            TempData["Success"] = trangThai == "DaDuyet"
                ? $"Đã phê duyệt sân {san.TenSan}!"
                : $"Đã từ chối sân {san.TenSan}!";
            return RedirectToAction("DuyetSan");
        }

        // GET /Admin/QuanLyUser
        public async Task<IActionResult> QuanLyUser(string? vaiTro)
        {
            var query = _context.Users.AsQueryable();
            if (!string.IsNullOrEmpty(vaiTro))
                query = query.Where(u => u.VaiTro == vaiTro);
            var list = await query
                .OrderByDescending(u => u.NgayTao)
                .ToListAsync();
            ViewBag.VaiTro = vaiTro;
            return View(list);
        }

        // GET /Admin/BaoCao
        public async Task<IActionResult> BaoCao(int? nam)
        {
            nam ??= DateTime.Now.Year;
            ViewBag.Nam = nam;

            // Doanh thu 12 tháng của năm đó
            var doanhThu = new List<object>();
            for (int thang = 1; thang <= 12; thang++)
            {
                var batDau = new DateTime(nam.Value, thang, 1);
                var ketThuc = batDau.AddMonths(1);
                var dt = await _context.DatSans
                    .Where(d => d.ThoiGianTao >= batDau
                             && d.ThoiGianTao < ketThuc
                             && d.TrangThai != "DaHuy")
                    .SumAsync(d => (decimal?)d.TienCoc) ?? 0;
                var soLuot = await _context.DatSans
                    .CountAsync(d => d.ThoiGianTao >= batDau
                                  && d.ThoiGianTao < ketThuc);
                doanhThu.Add(new
                {
                    thang = $"T{thang}",
                    dt = (double)dt,
                    soLuot
                });
            }
            ViewBag.DoanhThuNam = doanhThu;

            // Tỷ lệ lấp đầy theo loại sân
            ViewBag.TyLe = await _context.SanBongs
                .Include(s => s.KhungGios)
                .Select(s => new {
                    Ten = s.TenSan,
                    Tong = s.KhungGios.Count,
                    DaDat = s.KhungGios.Count(k => k.TrangThai == "DaDat")
                })
                .ToListAsync();

            return View();
        }
    }
}