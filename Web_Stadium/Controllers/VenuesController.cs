using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_Stadium.EFCore;
using Web_Stadium.End;
using Web_Stadium.Filters;

namespace Web_Stadium.Controllers
{
    public class VenuesController : Controller
    {
        private readonly SanBongContext _context;
        private readonly IRepository<SanBong> _sanBongRepo;
        private readonly IConfiguration _config;

        public VenuesController(
            SanBongContext context,
            IRepository<SanBong> sanBongRepo,
            IConfiguration config)
        {
            _context = context;
            _sanBongRepo = sanBongRepo;
            _config = config;
        }

        // ══════════════════════════════════════════════════════════
        // GET: /Venues — Tìm kiếm sân (dùng Master Data từ DB)
        // ══════════════════════════════════════════════════════════
        public async Task<IActionResult> Index(
            string? quan, string? loaiSan,
            string? loaiCo, string? tuKhoa,
            decimal? giaTu, decimal? giaDen,
            string? sapXep)
        {
            // FIX 1: Chỉ hiển thị sân DaDuyet + không bị ẩn (IsHidden=false)
            var query = _context.SanBongs
                .Where(s => s.TrangThaiDuyet == "DaDuyet" && !s.IsHidden);

            if (!string.IsNullOrEmpty(quan))
                query = query.Where(s => s.Quan == quan);
            if (!string.IsNullOrEmpty(loaiSan))
                query = query.Where(s => s.LoaiSan == loaiSan);
            if (!string.IsNullOrEmpty(loaiCo))
                query = query.Where(s => s.LoaiCo == loaiCo);
            if (!string.IsNullOrEmpty(tuKhoa))
                query = query.Where(s => s.TenSan.Contains(tuKhoa)
                                      || s.DiaChi.Contains(tuKhoa)
                                      || s.Quan.Contains(tuKhoa));

            // Sắp xếp
            query = sapXep switch
            {
                "gia_tang" => query.OrderBy(s => s.KhungGios.Min(k => k.Gia)),
                "gia_giam" => query.OrderByDescending(s => s.KhungGios.Min(k => k.Gia)),
                "danh_gia" => query.OrderByDescending(s => s.DanhGiaTrungBinh),
                _ => query.OrderByDescending(s => s.DanhGiaTrungBinh)
            };

            var danhSach = await query
                .Include(s => s.KhungGios)
                .ToListAsync();

            // FIX 2: Dùng Master Data từ DB thay vì hardcode
            ViewBag.DanhSachQuan = await _context.DanhMucQuans
                .Where(q => q.IsActive).OrderBy(q => q.ThuTu).ToListAsync();
            ViewBag.DanhSachLoaiSan = await _context.DanhMucLoaiSans
                .Where(l => l.IsActive).ToListAsync();
            ViewBag.DanhSachLoaiCo = await _context.DanhMucLoaiCos
                .Where(l => l.IsActive).ToListAsync();

            ViewBag.Quan = quan;
            ViewBag.LoaiSan = loaiSan;
            ViewBag.LoaiCo = loaiCo;
            ViewBag.TuKhoa = tuKhoa;
            ViewBag.SapXep = sapXep;

            return View(danhSach);
        }

        // ══════════════════════════════════════════════════════════
        // GET: /Venues/Details/5
        // ══════════════════════════════════════════════════════════
        public async Task<IActionResult> Details(int id)
        {
            var sanBong = await _context.SanBongs
                .Include(s => s.KhungGios)
                .Include(s => s.DanhGia).ThenInclude(d => d.User)
                // FIX 3: Dịch vụ gắn với sân cụ thể (không lấy tất cả)
                .Include(s => s.DichVus).ThenInclude(d => d.DanhMucDichVu)
                .FirstOrDefaultAsync(s => s.Id == id
                                       && s.TrangThaiDuyet == "DaDuyet"
                                       && !s.IsHidden);

            if (sanBong == null) return NotFound();

            // Cập nhật điểm TB
            if (sanBong.DanhGia.Any())
            {
                sanBong.DanhGiaTrungBinh = sanBong.DanhGia.Average(d => d.SoSao);
                await _context.SaveChangesAsync();
            }

            // FIX 3: Chỉ lấy dịch vụ còn tồn kho của sân này
            ViewBag.DichVus = sanBong.DichVus
                .Where(d => d.IsActive && d.TonKho > 0)
                .ToList();

            // Kiểm tra User đã đăng nhập chưa để hiển thị form đánh giá
            var userId = TokenHelper.LayUserId(Request, _config);
            if (userId.HasValue)
            {
                // FIX 4: Kiểm tra User có đơn HoanThanh tại sân này không
                var coDonHoanThanh = await _context.DatSans
                    .Include(d => d.KhungGio)
                    .AnyAsync(d => d.UserId == userId.Value
                               && d.KhungGio.SanBongId == id
                               && d.TrangThai == "HoanThanh");

                // Kiểm tra đã đánh giá chưa (theo đơn, không chỉ theo sân)
                var daDanhGia = await _context.DanhGias
                    .AnyAsync(dg => dg.UserId == userId.Value
                                 && dg.SanBongId == id);

                ViewBag.CoDonHoanThanh = coDonHoanThanh;
                ViewBag.DaDanhGia = daDanhGia;
                ViewBag.UserId = userId.Value;
            }

            return View(sanBong);
        }

        // ══════════════════════════════════════════════════════════
        // POST: Đánh giá sân — FIX 4: ràng buộc phải có đơn HoanThanh
        // ══════════════════════════════════════════════════════════
        [HttpPost]
        [YeuCauDangNhap]
        public async Task<IActionResult> DanhGiaSan(
            int sanBongId, int soSao, string? nhanXet)
        {
            var userId = TokenHelper.LayUserId(Request, _config);
            if (userId == null) return RedirectToAction("Login", "Auth");

            // FIX 4a: Phải có đơn HoanThanh tại sân này
            var donHoanThanh = await _context.DatSans
                .Include(d => d.KhungGio)
                .FirstOrDefaultAsync(d => d.UserId == userId.Value
                                       && d.KhungGio.SanBongId == sanBongId
                                       && d.TrangThai == "HoanThanh");

            if (donHoanThanh == null)
            {
                TempData["Error"] = "Bạn chỉ có thể đánh giá sau khi đã đến sân và hoàn thành trận đấu!";
                return RedirectToAction("Details", new { id = sanBongId });
            }

            // FIX 4b: Mỗi User chỉ đánh giá 1 lần tại 1 sân
            var daDanhGia = await _context.DanhGias
                .AnyAsync(dg => dg.UserId == userId.Value && dg.SanBongId == sanBongId);
            if (daDanhGia)
            {
                TempData["Error"] = "Bạn đã đánh giá sân này rồi!";
                return RedirectToAction("Details", new { id = sanBongId });
            }

            var danhGia = new DanhGia
            {
                UserId = userId.Value,
                SanBongId = sanBongId,
                DatSanId = donHoanThanh.Id,   // gắn với đơn cụ thể
                SoSao = Math.Clamp(soSao, 1, 5),
                NhanXet = nhanXet,
                NgayDanhGia = DateTime.Now
            };
            _context.DanhGias.Add(danhGia);

            // Cập nhật điểm TB ngay
            await _context.SaveChangesAsync();
            var allDG = await _context.DanhGias
                .Where(d => d.SanBongId == sanBongId).ToListAsync();
            var san = await _context.SanBongs.FindAsync(sanBongId);
            if (san != null)
            {
                san.DanhGiaTrungBinh = allDG.Average(d => d.SoSao);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Cảm ơn bạn đã đánh giá!";
            return RedirectToAction("Details", new { id = sanBongId });
        }
    }
}