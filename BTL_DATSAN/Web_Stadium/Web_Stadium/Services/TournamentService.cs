using Microsoft.EntityFrameworkCore;
using Web_Stadium.EFCore;

namespace Web_Stadium.Services
{
    /// <summary>
    /// Toàn bộ nghiệp vụ lifecycle giải đấu
    /// Controller chỉ gọi vào đây — không chứa logic
    /// </summary>
    public class TournamentService
    {
        private readonly SanBongContext _context;
        private readonly ScheduleService _scheduleService;
        private readonly StandingService _standingService;
        private readonly SuspensionService _suspensionService;
        private readonly TournamentNotificationService _notificationService;

        public TournamentService(
            SanBongContext context,
            ScheduleService scheduleService,
            StandingService standingService,
            SuspensionService suspensionService,
            TournamentNotificationService notificationService)
        {
            _context = context;
            _scheduleService = scheduleService;
            _standingService = standingService;
            _suspensionService = suspensionService;
            _notificationService = notificationService;
        }

        // ══════════════════════════════════════════════════════════
        // Tạo giải đấu + tự sinh các bảng A/B/C...
        // ══════════════════════════════════════════════════════════
        public async Task<(bool ok, string error, GiaiDau? giai)> TaoGiaiDau(
            CreateGiaiDauDto dto, int ownerId)
        {
            // Validate sân thuộc Owner
            var san = await _context.SanBongs.FirstOrDefaultAsync(s =>
                s.Id == dto.SanBongId &&
                s.OwnerId == ownerId &&
                s.TrangThaiDuyet == "DaDuyet" &&
                !s.IsHidden);

            if (san == null)
                return (false, "Sân không hợp lệ hoặc không thuộc quyền quản lý của bạn!", null);

            if (dto.NgayBatDau < DateTime.Today)
                return (false, "Ngày bắt đầu không được là ngày đã qua!", null);

            if (dto.NgayKetThuc <= dto.NgayBatDau)
                return (false, "Ngày kết thúc phải sau ngày bắt đầu!", null);

            var soDoiHopLe = new[] { 4, 8, 16, 32 };
            if (!soDoiHopLe.Contains(dto.SoDoiToiDa))
                return (false, "Số đội tối đa phải là 4, 8, 16 hoặc 32!", null);

            // Tạo giải
            var giai = new GiaiDau
            {
                TenGiai = dto.TenGiai.Trim(),
                MoTa = dto.MoTa?.Trim(),
                SanBongId = dto.SanBongId,
                OwnerId = ownerId,
                SoDoiToiDa = dto.SoDoiToiDa,
                SoBang = dto.SoBang,
                LePhiGiai = dto.LePhiGiai,
                TienKyQuy = dto.TienKyQuy,
                TienPhatTheVang = dto.TienPhatTheVang > 0 ? dto.TienPhatTheVang : 20000m,
                TienPhatTheDo = dto.TienPhatTheDo > 0 ? dto.TienPhatTheDo : 100000m,
                SoTranTreoGioTheDo = dto.SoTranTreoGioTheDo > 0 ? dto.SoTranTreoGioTheDo : 1,
                SoTheVangTichLuy = dto.SoTheVangTichLuy > 0 ? dto.SoTheVangTichLuy : 2,
                NgayBatDau = dto.NgayBatDau,
                NgayKetThuc = dto.NgayKetThuc,
                ThoiGianDongDanhSach = dto.ThoiGianDong ?? dto.NgayBatDau.AddDays(-1),
                TrangThai = "Draft",
                ThoiGianTao = DateTime.Now
            };

            _context.GiaiDaus.Add(giai);
            await _context.SaveChangesAsync();

            // Tự sinh bảng A, B, C...
            for (int i = 0; i < dto.SoBang; i++)
            {
                _context.BangDaus.Add(new BangDau
                {
                    GiaiDauId = giai.Id,
                    TenBang = "Bảng " + (char)('A' + i)
                });
            }
            await _context.SaveChangesAsync();

            return (true, "", giai);
        }

        // ══════════════════════════════════════════════════════════
        // Mở đăng ký: Approved → RegistrationOpen
        // ══════════════════════════════════════════════════════════
        public async Task<(bool ok, string error)> MoiDangKy(int giaiId, int ownerId)
        {
            var giai = await LayGiaiCuaOwner(giaiId, ownerId);
            if (giai == null) return (false, "Không tìm thấy giải!");

            if (giai.TrangThai == "Draft")
                return (false, "Giải đấu chưa được Admin phê duyệt, vui lòng chờ Admin duyệt trước");

            if (giai.TrangThai != "Approved")
                return (false, "Chỉ mở đăng ký khi giải đã được Admin phê duyệt!");

            giai.TrangThai = "RegistrationOpen";
            await _context.SaveChangesAsync();
            return (true, "");
        }

        // ══════════════════════════════════════════════════════════
        // Đóng đăng ký: RegistrationOpen → RegistrationClosed
        // ══════════════════════════════════════════════════════════
        public async Task<(bool ok, string error)> DongDangKy(int giaiId, int ownerId)
        {
            var giai = await _context.GiaiDaus
                .Include(g => g.DoiBongs)
                .FirstOrDefaultAsync(g => g.Id == giaiId && g.OwnerId == ownerId);

            if (giai == null) return (false, "Không tìm thấy giải!");
            if (giai.TrangThai != "RegistrationOpen")
                return (false, "Chỉ đóng đăng ký khi giải đang mở!");

            var soDoiHopLe = giai.DoiBongs.Count(d => d.DaThanhToan);
            if (soDoiHopLe < 2)
                return (false, "Cần ít nhất 2 đội đã thanh toán để đóng đăng ký!");

            giai.TrangThai = "RegistrationClosed";
            giai.ThoiGianDongDanhSach = DateTime.Now;
            await _context.SaveChangesAsync();
            return (true, "");
        }

        // ══════════════════════════════════════════════════════════
        // Gán đội vào bảng (Drag & Drop)
        // ══════════════════════════════════════════════════════════
        public async Task<(bool ok, string error)> GanDoiVaoBang(
            int doiId, int? bangId, int ownerId)
        {
            var doi = await _context.DoiBongs
                .Include(d => d.GiaiDau)
                .FirstOrDefaultAsync(d => d.Id == doiId && d.GiaiDau.OwnerId == ownerId);

            if (doi == null) return (false, "Không tìm thấy đội!");
            if (doi.GiaiDau.TrangThai != "RegistrationClosed")
                return (false, "Chỉ chia bảng khi đã đóng đăng ký!");

            if (bangId.HasValue)
            {
                var bang = await _context.BangDaus
                    .FirstOrDefaultAsync(b => b.Id == bangId && b.GiaiDauId == doi.GiaiDauId);
                if (bang == null) return (false, "Bảng không hợp lệ!");
            }

            doi.BangId = bangId;
            await _context.SaveChangesAsync();
            return (true, "");
        }

        // ══════════════════════════════════════════════════════════
        // Khởi tạo giải: sinh lịch + khóa slot + gửi email
        // RegistrationClosed → Active
        // ══════════════════════════════════════════════════════════
        public async Task<(bool ok, string error)> KhoiTaoGiai(int giaiId, int ownerId)
        {
            var giai = await _context.GiaiDaus
                .Include(g => g.BangDaus)
                .Include(g => g.DoiBongs).ThenInclude(d => d.Bang)
                .Include(g => g.DoiBongs).ThenInclude(d => d.DoiTruong)
                .Include(g => g.SanBong).ThenInclude(s => s.KhungGios)
                .FirstOrDefaultAsync(g => g.Id == giaiId && g.OwnerId == ownerId);

            if (giai == null) return (false, "Không tìm thấy giải!");
            if (giai.TrangThai != "RegistrationClosed")
                return (false, "Chỉ khởi tạo sau khi đóng đăng ký!");

            // Validate tất cả đội đã vào bảng
            var doiChuaBang = giai.DoiBongs.Where(d => d.BangId == null && d.DaThanhToan).ToList();
            if (doiChuaBang.Any())
                return (false, $"Còn {doiChuaBang.Count} đội chưa được xếp bảng!");

            // 1. Sinh lịch thi đấu (Berger Algorithm)
            var tranDaus = _scheduleService.SinhLichVongTron(giai);
            _context.TranDaus.AddRange(tranDaus);

            // DummyBooking đã bỏ — Owner tự đặt sân từng trận
            // Slot tự nhiên bị khóa khi Owner đặt sân bình thường

            // 2. Cập nhật trạng thái
            giai.TrangThai = "Active";
            await _context.SaveChangesAsync();

            return (true, "");
        }

        // ══════════════════════════════════════════════════════════
        // Kết thúc giải: Active → Finished
        // Giải phóng slot sân, tính phạt thẻ
        // ══════════════════════════════════════════════════════════
        public async Task<(bool ok, string error)> KetThucGiai(int giaiId, int ownerId)
        {
            var giai = await _context.GiaiDaus
                .Include(g => g.TranDaus)
                .FirstOrDefaultAsync(g => g.Id == giaiId && g.OwnerId == ownerId);

            if (giai == null) return (false, "Không tìm thấy giải!");
            if (giai.TrangThai != "Active")
                return (false, "Chỉ kết thúc khi giải đang Active!");

            var tranChuaXong = giai.TranDaus
                .Count(t => t.TrangThai is "Scheduled" or "InProgress");
            if (tranChuaXong > 0)
                return (false, $"Còn {tranChuaXong} trận chưa kết thúc!");

            giai.TrangThai = "Finished";

            // Giải phóng DummyBooking → mở lại slot sân

            await _context.SaveChangesAsync();
            return (true, "");
        }

        // ══════════════════════════════════════════════════════════
        // Xử lý sự cố: đội bỏ cuộc → xử thua 0-3
        // ══════════════════════════════════════════════════════════
        public async Task<(bool ok, string error)> XuLySuCo(
            int tranDauId, int doiBoCuocId, string lyDo, int ownerId)
        {
            var tran = await _context.TranDaus
                .Include(t => t.GiaiDau)
                .FirstOrDefaultAsync(t => t.Id == tranDauId && t.GiaiDau.OwnerId == ownerId);

            if (tran == null) return (false, "Không tìm thấy trận!");
            if (tran.TrangThai == "Closed") return (false, "Trận đã kết thúc!");
            if (tran.DoiNhaId != doiBoCuocId && tran.DoiKhachId != doiBoCuocId)
                return (false, "Đội không tham gia trận này!");
            if (string.IsNullOrWhiteSpace(lyDo))
                return (false, "Phải nhập lý do xử lý sự cố!");

            bool doiNhaBoCuoc = doiBoCuocId == tran.DoiNhaId;
            tran.BanThangNha = doiNhaBoCuoc ? 0 : 3;
            tran.BanThangKhach = doiNhaBoCuoc ? 3 : 0;
            tran.TrangThai = "Closed";

            _context.SuKienTrans.Add(new SuKienTran
            {
                TranDauId = tranDauId,
                DoiId = doiBoCuocId,
                LoaiSuKien = "SuCo",
                GhiChu = $"Bỏ cuộc. Lý do: {lyDo}",
                ThoiGianGhi = DateTime.Now
            });

            // Trừ 100% ký quỹ
            var doi = await _context.DoiBongs.FindAsync(doiBoCuocId);
            if (doi != null) doi.TienKyQuyConLai = 0;

            await _context.SaveChangesAsync();

            // Kiểm tra treo giò sau sự cố
            await _suspensionService.XuLyTreoGio(tran.GiaiDauId);

            return (true, "");
        }

        // ── Helper ─────────────────────────────────────────────
        private Task<GiaiDau?> LayGiaiCuaOwner(int giaiId, int ownerId)
            => _context.GiaiDaus.FirstOrDefaultAsync(g => g.Id == giaiId && g.OwnerId == ownerId);
    }

    // ── DTOs ─────────────────────────────────────────────────────
    public class CreateGiaiDauDto
    {
        public string TenGiai { get; set; } = "";
        public string? MoTa { get; set; }
        public int SanBongId { get; set; }
        public int SoDoiToiDa { get; set; } = 8;
        public int SoBang { get; set; } = 2;
        public decimal LePhiGiai { get; set; }
        public decimal TienKyQuy { get; set; }
        public decimal TienPhatTheVang { get; set; } = 20000m;
        public decimal TienPhatTheDo { get; set; } = 100000m;
        public int SoTranTreoGioTheDo { get; set; } = 1;
        public int SoTheVangTichLuy { get; set; } = 2;
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public DateTime? ThoiGianDong { get; set; }
    }
}

// ══════════════════════════════════════════════════════════════
// THÊM VÀO TournamentService — delegate sang KnockOutService
// ══════════════════════════════════════════════════════════════
/*
// Inject thêm KnockOutService vào constructor:
private readonly KnockOutService _knockOutService;

// Thêm method:
public async Task<(bool ok, string error)> SinhVongKnockOut(int giaiDauId)
    => await _knockOutService.SinhVongKnockOut(giaiDauId);
*/

// ══════════════════════════════════════════════════════════════
// THÊM VÀO TournamentController — Owner xác nhận đã nhận tiền
// ══════════════════════════════════════════════════════════════
/*
[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> XacNhanThanhToanDoi(int doiId)
{
    var doi = await _context.DoiBongs
        .Include(d => d.GiaiDau)
        .Include(d => d.DoiTruong)
        .FirstOrDefaultAsync(d => d.Id == doiId
                              && d.GiaiDau.OwnerId == OwnerId());
    if (doi == null) return NotFound();

    if (doi.DaThanhToan)
    {
        TempData["Error"] = "Đội này đã được xác nhận rồi!";
        return RedirectToAction("Details", new { id = doi.GiaiDauId });
    }

    doi.DaThanhToan       = true;
    doi.ThoiGianThanhToan = DateTime.Now;
    await _context.SaveChangesAsync();

    // Gửi email xác nhận cho đội trưởng — SAU KHI Owner confirm
    var publicCtrl = new TournamentPublicController(_context, _config, _emailService);
    _ = publicCtrl.GuiEmailXacNhanDangKy(doi);

    await GhiLog("XacNhanThanhToan", "DoiBong", doiId,
        $"Owner xác nhận đã nhận tiền từ đội '{doi.TenDoi}'");

    TempData["Success"] = $"✅ Đã xác nhận thanh toán cho đội '{doi.TenDoi}'!";
    return RedirectToAction("Details", new { id = doi.GiaiDauId });
}

[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> HuyDangKyDoi(int doiId, string lyDo)
{
    var doi = await _context.DoiBongs
        .Include(d => d.GiaiDau)
        .FirstOrDefaultAsync(d => d.Id == doiId
                              && d.GiaiDau.OwnerId == OwnerId());
    if (doi == null) return NotFound();

    if (doi.GiaiDau.TrangThai != "RegistrationOpen")
    {
        TempData["Error"] = "Chỉ hủy đăng ký khi giải đang mở!";
        return RedirectToAction("Details", new { id = doi.GiaiDauId });
    }

    var giaiId = doi.GiaiDauId;
    _context.DoiBongs.Remove(doi);
    await _context.SaveChangesAsync();

    await GhiLog("HuyDangKyDoi", "DoiBong", doiId,
        $"Hủy đăng ký đội '{doi.TenDoi}'. Lý do: {lyDo}");

    TempData["Success"] = $"Đã hủy đăng ký đội '{doi.TenDoi}'.";
    return RedirectToAction("Details", new { id = giaiId });
}
*/