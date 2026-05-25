using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_Stadium.EFCore;
using Web_Stadium.Filters;
using Web_Stadium.Services;

namespace Web_Stadium.Controllers
{
    /// <summary>
    /// Tournament Controller — chỉ làm 3 việc:
    /// 1. Nhận request HTTP
    /// 2. Gọi Service xử lý
    /// 3. Trả về View hoặc redirect
    /// KHÔNG chứa logic nghiệp vụ
    /// </summary>
    [YeuCauDangNhap]
    public class TournamentController : Controller
    {
        private readonly SanBongContext _context;
        private readonly IConfiguration _config;
        private readonly TournamentService _tournamentService;
        private readonly StandingService _standingService;
        private readonly TournamentExcelService _excelService;

        public TournamentController(
            SanBongContext context,
            IConfiguration config,
            TournamentService tournamentService,
            StandingService standingService,
            TournamentExcelService excelService)
        {
            _context = context;
            _config = config;
            _tournamentService = tournamentService;
            _standingService = standingService;
            _excelService = excelService;
        }

        private int OwnerId() => TokenHelper.LayUserId(Request, _config)!.Value;

        private async Task GhiLog(string hanhDong, string doiTuong, int id, string moTa)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                UserId = OwnerId(),
                VaiTro = "Owner",
                HanhDong = hanhDong,
                DoiTuong = doiTuong,
                DoiTuongId = id,
                MoTa = moTa,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            });
            await _context.SaveChangesAsync();
        }

        // ── GET /Tournament ──────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var list = await _context.GiaiDaus
                .Include(g => g.SanBong)
                .Include(g => g.DoiBongs)
                .Include(g => g.TranDaus)
                .Where(g => g.OwnerId == OwnerId())
                .OrderByDescending(g => g.ThoiGianTao)
                .ToListAsync();

            return View(list);
        }

        // ── GET /Tournament/Create ───────────────────────────────
        public async Task<IActionResult> Create()
        {
            ViewBag.SanList = await _context.SanBongs
                .Where(s => s.OwnerId == OwnerId()
                         && s.TrangThaiDuyet == "DaDuyet"
                         && !s.IsHidden)
                .ToListAsync();
            return View();
        }

        // ── POST /Tournament/Create ──────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateGiaiDauDto dto)
        {
            var (ok, error, giai) = await _tournamentService.TaoGiaiDau(dto, OwnerId());
            if (!ok)
            {
                TempData["Error"] = error;
                ViewBag.SanList = await _context.SanBongs
                    .Where(s => s.OwnerId == OwnerId()
                             && s.TrangThaiDuyet == "DaDuyet"
                             && !s.IsHidden).ToListAsync();
                return View(dto);
            }

            await GhiLog("TaoGiaiDau", "GiaiDau", giai!.Id, $"Tạo giải '{giai.TenGiai}'");
            TempData["Success"] = $"Tạo giải '{giai.TenGiai}' thành công!";
            return RedirectToAction("Details", new { id = giai.Id });
        }

        // ── GET /Tournament/Details/5 ────────────────────────────
        public async Task<IActionResult> Details(int id)
        {
            var giai = await _context.GiaiDaus
                .Include(g => g.SanBong)
                .Include(g => g.BangDaus)
                .Include(g => g.DoiBongs).ThenInclude(d => d.ThanhViens)
                .Include(g => g.DoiBongs).ThenInclude(d => d.Bang)
                .Include(g => g.DoiBongs).ThenInclude(d => d.DoiTruong)
                .Include(g => g.TranDaus).ThenInclude(t => t.DoiNha)
                .Include(g => g.TranDaus).ThenInclude(t => t.DoiKhach)
                .Include(g => g.TranDaus).ThenInclude(t => t.SuKiens)
                .FirstOrDefaultAsync(g => g.Id == id && g.OwnerId == OwnerId());

            if (giai == null) return NotFound();

            // BXH do StandingService tính — View chỉ render
            ViewBag.BangXepHang = await _standingService.GetStandings(id);
            return View(giai);
        }

        // ── POST /Tournament/MoiDangKy ───────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MoiDangKy(int id)
        {
            var (ok, error) = await _tournamentService.MoiDangKy(id, OwnerId());
            if (!ok) { TempData["Error"] = error; return RedirectToAction("Details", new { id }); }

            await GhiLog("MoiDangKy", "GiaiDau", id, "Mở đăng ký giải");
            TempData["Success"] = "Đã mở đăng ký!";
            return RedirectToAction("Details", new { id });
        }

        // ── POST /Tournament/DongDangKy ──────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DongDangKy(int id)
        {
            var (ok, error) = await _tournamentService.DongDangKy(id, OwnerId());
            if (!ok) { TempData["Error"] = error; return RedirectToAction("Details", new { id }); }

            await GhiLog("DongDangKy", "GiaiDau", id, "Đóng đăng ký giải");
            TempData["Success"] = "Đã đóng đăng ký! Tiến hành chia bảng.";
            return RedirectToAction("ChiaBang", new { id });
        }

        // ── GET /Tournament/ChiaBang/5 ───────────────────────────
        public async Task<IActionResult> ChiaBang(int id)
        {
            var giai = await _context.GiaiDaus
                .Include(g => g.BangDaus)
                .Include(g => g.DoiBongs).ThenInclude(d => d.Bang)
                .Include(g => g.DoiBongs).ThenInclude(d => d.DoiTruong)
                .Include(g => g.DoiBongs).ThenInclude(d => d.ThanhViens)
                .FirstOrDefaultAsync(g => g.Id == id && g.OwnerId == OwnerId());

            if (giai == null) return NotFound();
            if (giai.TrangThai != "RegistrationClosed")
            {
                TempData["Error"] = "Chỉ chia bảng sau khi đóng đăng ký!";
                return RedirectToAction("Details", new { id });
            }
            return View(giai);
        }

        // ── POST /Tournament/GanBang (AJAX) ──────────────────────
        [HttpPost]
        public async Task<IActionResult> GanBang(int doiId, int? bangId)
        {
            var (ok, error) = await _tournamentService.GanDoiVaoBang(doiId, bangId, OwnerId());
            if (!ok) return Json(new { ok = false, message = error });

            var doi = await _context.DoiBongs.FindAsync(doiId);
            var bang = bangId.HasValue
                ? await _context.BangDaus.FindAsync(bangId)
                : null;

            return Json(new { ok = true, tenDoi = doi?.TenDoi, tenBang = bang?.TenBang });
        }

        // ── POST /Tournament/KhoiTao ─────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> KhoiTao(int id)
        {
            var (ok, error) = await _tournamentService.KhoiTaoGiai(id, OwnerId());
            if (!ok) { TempData["Error"] = error; return RedirectToAction("ChiaBang", new { id }); }

            await GhiLog("KhoiTaoGiai", "GiaiDau", id, "Khởi tạo giải và sinh lịch thi đấu");
            TempData["Success"] = "Khởi tạo thành công! Email lịch đấu đã gửi cho các đội.";
            return RedirectToAction("Details", new { id });
        }

        // ── POST /Tournament/KetThuc ─────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> KetThuc(int id)
        {
            var (ok, error) = await _tournamentService.KetThucGiai(id, OwnerId());
            if (!ok) { TempData["Error"] = error; return RedirectToAction("Details", new { id }); }

            await GhiLog("KetThucGiai", "GiaiDau", id, "Kết thúc giải đấu");
            TempData["Success"] = "Giải kết thúc! Tải Excel đối soát bên dưới.";
            return RedirectToAction("Details", new { id });
        }

        // ── POST /Tournament/XuLySuCo ────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> XuLySuCo(int tranDauId, int doiBoCuocId, string lyDo)
        {
            var (ok, error) = await _tournamentService.XuLySuCo(
                tranDauId, doiBoCuocId, lyDo, OwnerId());

            var tran = await _context.TranDaus.FindAsync(tranDauId);
            if (!ok) { TempData["Error"] = error; }
            else
            {
                await GhiLog("XuLySuCo", "TranDau", tranDauId,
                    $"Đội {doiBoCuocId} bỏ cuộc. Lý do: {lyDo}");
                TempData["Success"] = "Đã xử lý sự cố. Đội vi phạm thua 0-3.";
            }

            return RedirectToAction("Details", new { id = tran?.GiaiDauId });
        }

        // ── GET /Tournament/ExcelDoiSoat/5 ──────────────────────
        public async Task<IActionResult> ExcelDoiSoat(int id)
        {
            var giai = await _context.GiaiDaus
                .FirstOrDefaultAsync(g => g.Id == id && g.OwnerId == OwnerId());
            if (giai == null) return NotFound();

            var (bytes, fileName) = await _excelService.ExportDoiSoat(id);
            await GhiLog("ExportExcel", "GiaiDau", id, "Xuất Excel đối soát tài chính");

            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
    }
}

// ══════════════════════════════════════════════════════════════
// THÊM VÀO Program.cs — Đăng ký tất cả Tournament Services
// Đặt sau dòng: builder.Services.AddScoped<EmailService>();
// ══════════════════════════════════════════════════════════════

/*
builder.Services.AddScoped<ScheduleService>();
builder.Services.AddScoped<StandingService>();
builder.Services.AddScoped<SuspensionService>();
builder.Services.AddScoped<TournamentNotificationService>();
builder.Services.AddScoped<TournamentExcelService>();
builder.Services.AddScoped<TournamentService>();
*/

// ══════════════════════════════════════════════════════════════
// PATCH: Thêm các action sau vào TournamentController
// ══════════════════════════════════════════════════════════════

/*
// ── POST /Tournament/GanKhungGio — Owner gán slot sân cho trận ──
[HttpPost]
public async Task<IActionResult> GanKhungGio(
    int tranDauId, int khungGioId, string ngayThiDauStr)
{
    var tran = await _context.TranDaus
        .Include(t => t.GiaiDau)
        .FirstOrDefaultAsync(t => t.Id == tranDauId
                               && t.GiaiDau.OwnerId == OwnerId());
    if (tran == null)
        return Json(new { ok = false, message = "Không tìm thấy trận!" });

    if (tran.TrangThai != "Scheduled")
        return Json(new { ok = false, message = "Trận đã bắt đầu, không thể đổi giờ!" });

    if (!DateTime.TryParse(ngayThiDauStr, out var ngay))
        return Json(new { ok = false, message = "Ngày không hợp lệ!" });

    // Kiểm tra slot còn trống
    var kg = await _context.KhungGios
        .Include(k => k.SanBong)
        .FirstOrDefaultAsync(k => k.Id == khungGioId
                              && k.SanBong.Id == tran.GiaiDau.SanBongId);
    if (kg == null)
        return Json(new { ok = false, message = "Khung giờ không thuộc sân này!" });

    // Kiểm tra slot ngày đó chưa bị trận khác dùng
    var trungLich = await _context.TranDaus.AnyAsync(t =>
        t.Id != tranDauId &&
        t.GiaiDauId == tran.GiaiDauId &&
        t.KhungGioId == khungGioId &&
        t.NgayThiDau.Date == ngay.Date);

    if (trungLich)
        return Json(new { ok = false, message = "Khung giờ này đã có trận khác trong cùng ngày!" });

    tran.KhungGioId  = khungGioId;
    tran.NgayThiDau  = ngay;
    await _context.SaveChangesAsync();

    return Json(new {
        ok    = true,
        gioBD = kg.GioBatDau.ToString("HH:mm"),
        gioKT = kg.GioKetThuc.ToString("HH:mm"),
        ngay  = ngay.ToString("dd/MM/yyyy")
    });
}

// ── POST /Tournament/GanStaff — Owner gán Staff phụ trách giải ─
[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> GanStaff(int giaiDauId, int staffId)
{
    var giai = await _context.GiaiDaus
        .Include(g => g.TranDaus)
        .FirstOrDefaultAsync(g => g.Id == giaiDauId && g.OwnerId == OwnerId());
    if (giai == null) return NotFound();

    // Validate Staff thuộc sân của Owner
    var sanCuaToi = await _context.SanBongs
        .Where(s => s.OwnerId == OwnerId())
        .Select(s => s.Id)
        .ToListAsync();

    var staffHopLe = await _context.StaffSanPhanCongs
        .AnyAsync(p => p.StaffId == staffId && sanCuaToi.Contains(p.SanBongId));

    if (!staffHopLe)
    {
        TempData["Error"] = "Staff không được phân công tại sân của bạn!";
        return RedirectToAction("Details", new { id = giaiDauId });
    }

    // Gán Staff cho tất cả trận chưa có người phụ trách
    foreach (var tran in giai.TranDaus.Where(t => t.StaffPhuTrachId == null))
        tran.StaffPhuTrachId = staffId;

    await _context.SaveChangesAsync();
    await GhiLog("GanStaff", "GiaiDau", giaiDauId, $"Gán Staff {staffId} phụ trách giải");

    TempData["Success"] = "Đã gán Staff phụ trách toàn bộ trận đấu!";
    return RedirectToAction("Details", new { id = giaiDauId });
}

// ── POST /Tournament/SinhBracket — Sinh vòng knock-out sau vòng bảng ─
[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> SinhBracket(int giaiDauId)
{
    var giai = await _context.GiaiDaus
        .Include(g => g.BangDaus)
        .Include(g => g.DoiBongs)
        .Include(g => g.TranDaus)
        .FirstOrDefaultAsync(g => g.Id == giaiDauId && g.OwnerId == OwnerId());

    if (giai == null) return NotFound();

    // Kiểm tra tất cả trận vòng bảng đã Closed
    var tranBangChuaXong = giai.TranDaus
        .Count(t => t.LoaiVong == "VongBang" && t.TrangThai != "Closed");
    if (tranBangChuaXong > 0)
    {
        TempData["Error"] = $"Còn {tranBangChuaXong} trận vòng bảng chưa kết thúc!";
        return RedirectToAction("Details", new { id = giaiDauId });
    }

    // Gọi service sinh bracket
    var bracket = await _tournamentService.SinhVongKnockOut(giaiDauId);
    if (!bracket.ok)
    {
        TempData["Error"] = bracket.error;
        return RedirectToAction("Details", new { id = giaiDauId });
    }

    await GhiLog("SinhBracket", "GiaiDau", giaiDauId, "Sinh vòng knock-out");
    TempData["Success"] = "Đã sinh lịch vòng knock-out!";
    return RedirectToAction("Details", new { id = giaiDauId });
}
*/