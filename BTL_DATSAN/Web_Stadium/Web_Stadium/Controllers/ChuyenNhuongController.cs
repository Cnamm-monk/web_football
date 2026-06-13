using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_Stadium.EFCore;
using Web_Stadium.Filters;
using Web_Stadium.Services;

namespace Web_Stadium.Controllers
{
    public class ChuyenNhuongController : Controller
    {
        private readonly SanBongContext _context;
        private readonly IConfiguration _config;
        private readonly EmailService _emailService;

        public ChuyenNhuongController(SanBongContext context, IConfiguration config, EmailService emailService)
        {
            _context = context;
            _config = config;
            _emailService = emailService;
        }

        private int? CurrentUserId() => TokenHelper.LayUserId(Request, _config);

        // ══════════════════════════════════════════════════════════
        // GET /ChuyenNhuong/Index — danh sách bài đăng
        // ══════════════════════════════════════════════════════════
        public async Task<IActionResult> Index(string? quan, string? loaiSan, string? ngay, decimal? giaMin, decimal? giaMax)
        {
            var query = _context.ChuyenNhuongs
                .Include(c => c.DatSan)
                    .ThenInclude(d => d.KhungGio).ThenInclude(k => k.SanBong)
                .Include(c => c.UserA)
                .Where(c => c.TrangThai == "DangTim")
                .AsQueryable();

            if (!string.IsNullOrEmpty(quan))
                query = query.Where(c => c.DatSan.KhungGio.SanBong.Quan == quan);

            if (!string.IsNullOrEmpty(loaiSan))
                query = query.Where(c => c.DatSan.KhungGio.SanBong.LoaiSan == loaiSan);

            if (!string.IsNullOrEmpty(ngay) && DateTime.TryParse(ngay, out var ngayFilter))
                query = query.Where(c => c.DatSan.NgayThiDau.Date == ngayFilter.Date);

            if (giaMin.HasValue)
                query = query.Where(c => c.GiaChuyenNhuong >= giaMin.Value);

            if (giaMax.HasValue)
                query = query.Where(c => c.GiaChuyenNhuong <= giaMax.Value);

            var list = await query.OrderByDescending(c => c.ThoiGianTao).ToListAsync();

            var dsDiaChi = await _context.SanBongs.Select(s => s.Quan).Distinct().OrderBy(q => q).ToListAsync();
            ViewBag.DsQuan = dsDiaChi;
            ViewBag.CurrentUserId = CurrentUserId();
            ViewBag.Filter = new { quan, loaiSan, ngay, giaMin, giaMax };

            return View(list);
        }

        // ══════════════════════════════════════════════════════════
        // GET /ChuyenNhuong/DangBai/{datSanId}
        // ══════════════════════════════════════════════════════════
        [YeuCauDangNhap]
        public async Task<IActionResult> DangBai(int datSanId)
        {
            var userId = CurrentUserId();
            var don = await _context.DatSans
                .Include(d => d.KhungGio).ThenInclude(k => k.SanBong).ThenInclude(s => s.DichVus)
                .Include(d => d.DatSanDichVus).ThenInclude(dd => dd.DichVu)
                .FirstOrDefaultAsync(d => d.Id == datSanId && d.UserId == userId);

            if (don == null) return NotFound();
            if (don.TrangThai != "DaXacNhan")
            {
                TempData["Error"] = "Chỉ có thể chuyển nhượng đơn đã xác nhận.";
                return RedirectToAction("MyBookings", "Booking");
            }

            var daCoTin = await _context.ChuyenNhuongs
                .AnyAsync(c => c.DatSanId == datSanId && c.TrangThai == "DangTim");
            if (daCoTin)
            {
                TempData["Error"] = "Đơn này đã có bài đăng chuyển nhượng đang hoạt động.";
                return RedirectToAction("MyBookings", "Booking");
            }

            var daChuyenNhuong = await _context.ChuyenNhuongs
                .AnyAsync(c => c.DatSanId == datSanId && c.DaChuyenNhuong);
            if (daChuyenNhuong)
            {
                TempData["Error"] = "Đơn này đã từng được chuyển nhượng, không thể chuyển lại.";
                return RedirectToAction("MyBookings", "Booking");
            }

            ViewBag.DatSan = don;
            return View();
        }

        // ══════════════════════════════════════════════════════════
        // POST /ChuyenNhuong/GuiBai
        // ══════════════════════════════════════════════════════════
        [HttpPost]
        [YeuCauDangNhap]
        public async Task<IActionResult> GuiBai(int datSanId, string tieuDe, string? moTa, decimal giaChuyenNhuong)
        {
            var userId = CurrentUserId();
            var don = await _context.DatSans
                .FirstOrDefaultAsync(d => d.Id == datSanId && d.UserId == userId && d.TrangThai == "DaXacNhan");

            if (don == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hợp lệ.";
                return RedirectToAction("Index");
            }

            var daCoTin = await _context.ChuyenNhuongs
                .AnyAsync(c => c.DatSanId == datSanId && (c.TrangThai == "DangTim" || c.DaChuyenNhuong));
            if (daCoTin)
            {
                TempData["Error"] = "Đơn này không thể đăng chuyển nhượng.";
                return RedirectToAction("MyBookings", "Booking");
            }

            var cn = new ChuyenNhuong
            {
                DatSanId = datSanId,
                UserAId = userId!.Value,
                TieuDe = tieuDe.Trim(),
                MoTa = moTa?.Trim(),
                GiaChuyenNhuong = Math.Max(0, giaChuyenNhuong),
                TrangThai = "DangTim",
                ThoiGianTao = DateTime.Now
            };
            _context.ChuyenNhuongs.Add(cn);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã đăng bài chuyển nhượng thành công!";
            return RedirectToAction("Index");
        }

        // ══════════════════════════════════════════════════════════
        // GET /ChuyenNhuong/ChiTiet/{id}
        // ══════════════════════════════════════════════════════════
        public async Task<IActionResult> ChiTiet(int id)
        {
            var cn = await _context.ChuyenNhuongs
                .Include(c => c.DatSan)
                    .ThenInclude(d => d.KhungGio).ThenInclude(k => k.SanBong)
                .Include(c => c.DatSan)
                    .ThenInclude(d => d.DatSanDichVus).ThenInclude(dd => dd.DichVu)
                .Include(c => c.UserA)
                .Include(c => c.UserB)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cn == null) return NotFound();

            ViewBag.CurrentUserId = CurrentUserId();
            return View(cn);
        }

        // ══════════════════════════════════════════════════════════
        // POST /ChuyenNhuong/TiepNhan
        // ══════════════════════════════════════════════════════════
        [HttpPost]
        [YeuCauDangNhap]
        public async Task<IActionResult> TiepNhan(int chuyenNhuongId)
        {
            var userId = CurrentUserId();
            var cn = await _context.ChuyenNhuongs
                .Include(c => c.DatSan).ThenInclude(d => d.KhungGio).ThenInclude(k => k.SanBong)
                .Include(c => c.UserA)
                .FirstOrDefaultAsync(c => c.Id == chuyenNhuongId && c.TrangThai == "DangTim");

            if (cn == null)
            {
                TempData["Error"] = "Bài đăng không còn khả dụng.";
                return RedirectToAction("Index");
            }

            if (cn.UserAId == userId)
            {
                TempData["Error"] = "Bạn không thể tiếp nhận bài của chính mình.";
                return RedirectToAction("ChiTiet", new { id = chuyenNhuongId });
            }

            cn.UserBId = userId;
            cn.TrangThai = "ChoXacNhan";
            await _context.SaveChangesAsync();

            // Email + notification to User A
            var userA = cn.UserA;
            var san = cn.DatSan?.KhungGio?.SanBong;
            var userBName = (await _context.Users.FindAsync(userId))?.HoTen ?? "Ai đó";
            if (userA != null && !string.IsNullOrEmpty(userA.Email))
            {
                _ = Task.Run(() => _emailService.GuiEmailCoNguoiTiepNhan(
                    userA.Email, userA.HoTen ?? "Bạn", userBName,
                    san?.TenSan ?? "sân bóng",
                    cn.DatSan?.NgayThiDau.ToString("dd/MM/yyyy") ?? "",
                    $"{cn.DatSan?.KhungGio?.GioBatDau:HH\\:mm}–{cn.DatSan?.KhungGio?.GioKetThuc:HH\\:mm}"));
            }

            TempData["Success"] = "Đã tiếp nhận! Vui lòng chờ người chuyển nhượng xác nhận.";
            return RedirectToAction("ChiTiet", new { id = chuyenNhuongId });
        }

        // ══════════════════════════════════════════════════════════
        // GET /ChuyenNhuong/XacNhan/{id} [UserA]
        // ══════════════════════════════════════════════════════════
        [YeuCauDangNhap]
        public async Task<IActionResult> XacNhan(int id)
        {
            var userId = CurrentUserId();
            var cn = await _context.ChuyenNhuongs
                .Include(c => c.DatSan).ThenInclude(d => d.KhungGio).ThenInclude(k => k.SanBong)
                .Include(c => c.UserA)
                .Include(c => c.UserB)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserAId == userId && c.TrangThai == "ChoXacNhan");

            if (cn == null)
            {
                TempData["Error"] = "Không tìm thấy yêu cầu hợp lệ.";
                return RedirectToAction("Index");
            }

            return View(cn);
        }

        // ══════════════════════════════════════════════════════════
        // POST /ChuyenNhuong/XacNhanChuyenNhuong
        // ══════════════════════════════════════════════════════════
        [HttpPost]
        [YeuCauDangNhap]
        public async Task<IActionResult> XacNhanChuyenNhuong(int chuyenNhuongId, string hanhDong)
        {
            var userId = CurrentUserId();
            var cn = await _context.ChuyenNhuongs
                .Include(c => c.DatSan).ThenInclude(d => d.KhungGio).ThenInclude(k => k.SanBong)
                .Include(c => c.UserA)
                .Include(c => c.UserB)
                .FirstOrDefaultAsync(c => c.Id == chuyenNhuongId && c.UserAId == userId && c.TrangThai == "ChoXacNhan");

            if (cn == null) return NotFound();

            if (hanhDong == "TuChoi")
            {
                cn.TrangThai = "DangTim";
                cn.UserBId = null;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã từ chối. Bài đăng quay lại trạng thái đang tìm.";
                return RedirectToAction("ChiTiet", new { id = chuyenNhuongId });
            }

            // XacNhan → chuyển sang ChoStaff
            cn.TrangThai = "ChoStaff";
            cn.ThoiGianXuLy = DateTime.Now;
            await _context.SaveChangesAsync();

            // Email Staff
            var staffEmails = await _context.Users
                .Where(u => u.VaiTro == "Staff" && u.IsActive)
                .Select(u => new { u.Email, u.HoTen })
                .ToListAsync();

            var san = cn.DatSan?.KhungGio?.SanBong;
            foreach (var s in staffEmails)
            {
                if (!string.IsNullOrEmpty(s.Email))
                    _ = Task.Run(() => _emailService.GuiEmailChuyenNhuongChoStaff(
                        s.Email, s.HoTen ?? "Staff",
                        cn.UserA.HoTen ?? "A", cn.UserB?.HoTen ?? "B",
                        san?.TenSan ?? "", cn.DatSan?.NgayThiDau.ToString("dd/MM/yyyy") ?? "",
                        $"{cn.DatSan?.KhungGio?.GioBatDau:HH\\:mm}–{cn.DatSan?.KhungGio?.GioKetThuc:HH\\:mm}",
                        cn.GiaChuyenNhuong));
            }

            TempData["Success"] = "Đã xác nhận! Hệ thống đang chờ Staff kiểm duyệt.";
            return RedirectToAction("Index");
        }

        // ══════════════════════════════════════════════════════════
        // POST /ChuyenNhuong/HuyBai — UserA hủy bài đăng của mình
        // ══════════════════════════════════════════════════════════
        [HttpPost]
        [YeuCauDangNhap]
        public async Task<IActionResult> HuyBai(int chuyenNhuongId)
        {
            var userId = CurrentUserId();
            var cn = await _context.ChuyenNhuongs
                .FirstOrDefaultAsync(c => c.Id == chuyenNhuongId
                    && c.UserAId == userId
                    && c.TrangThai == "DangTim");

            if (cn == null) return NotFound();

            cn.TrangThai = "DaHuy";
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã hủy bài đăng chuyển nhượng.";
            return RedirectToAction("Index");
        }
    }
}
