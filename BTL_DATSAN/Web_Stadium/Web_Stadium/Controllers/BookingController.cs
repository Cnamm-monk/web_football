using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Web_Stadium.EFCore;
using Web_Stadium.Filters;
using Web_Stadium.Hubs;
using Web_Stadium.Services;

namespace Web_Stadium.Controllers
{
    public class BookingController : Controller
    {
        private readonly SanBongContext _context;
        private readonly IRepository<DatSan> _datSanRepo;
        private readonly IRepository<KhungGio> _khungGioRepo;
        private readonly IConfiguration _config;
        private readonly IHubContext<SanBongHub> _hub;
        private readonly EmailService _emailService;

        public BookingController(
            SanBongContext context,
            IRepository<DatSan> datSanRepo,
            IRepository<KhungGio> khungGioRepo,
            IConfiguration config,
            IHubContext<SanBongHub> hub,
            EmailService emailService)
        {
            _context = context;
            _datSanRepo = datSanRepo;
            _khungGioRepo = khungGioRepo;
            _config = config;
            _hub = hub;
            _emailService = emailService;
        }

        // ══════════════════════════════════════════════════════════
        // GET /Booking/Create?khungGioId=1&ngay=2024-04-15
        // ══════════════════════════════════════════════════════════
        [YeuCauDangNhap]
        public async Task<IActionResult> Create(int khungGioId, string? ngayStr)
        {
            var userId = TokenHelper.LayUserId(Request, _config);

            // Parse ngày an toàn — tránh SqlDateTime overflow
            if (!DateTime.TryParse(ngayStr, out var ngay) || ngay < new DateTime(1753, 1, 1))
                ngay = DateTime.Today;

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

            // Giữ chỗ 5 phút (theo flow)
            khungGio.TrangThai = "DangGiu";
            khungGio.ThoiGianHetGiuCho = DateTime.Now.AddMinutes(5);
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

            // Voucher user đang có (chưa dùng, chưa hết hạn)
            var vouchers = await _context.UserVouchers
                .Include(uv => uv.Voucher)
                .Where(uv => uv.UserId == userId
                          && !uv.IsUsed
                          && uv.NgayHetHan > DateTime.Now)
                .OrderBy(uv => uv.NgayHetHan)
                .ToListAsync();

            ViewBag.KhungGio = khungGio;
            ViewBag.Ngay = ngay;
            ViewBag.TyLeCoc = tyLeCoc;
            ViewBag.TienCoc = khungGio.Gia * tyLeCoc;
            ViewBag.DichVus = dichVus;
            ViewBag.Vouchers = vouchers;
            ViewBag.HetHan = khungGio.ThoiGianHetGiuCho;

            return View();
        }

        // ══════════════════════════════════════════════════════════
        // POST /Booking/Create
        // ══════════════════════════════════════════════════════════
        [HttpPost]
        [YeuCauDangNhap]
        public async Task<IActionResult> Create(
            int khungGioId,
            string? ngayThiDauStr,
            List<int>? dichVuIds,
            List<int>? soLuongs,
            string? userVoucherId)
        {
            // Parse ngày an toàn — tránh SqlDateTime overflow
            if (!DateTime.TryParse(ngayThiDauStr, out var ngayThiDau) || ngayThiDau < new DateTime(1753, 1, 1))
                ngayThiDau = DateTime.Today;

            // ❌ FIX 1: Không cho đặt ngày trong quá khứ
            if (ngayThiDau.Date < DateTime.Today)
            {
                TempData["Error"] = "Không thể đặt sân cho ngày đã qua. Vui lòng chọn ngày hôm nay hoặc tương lai!";
                return RedirectToAction("Details", "Venues", new { id = khungGioId });
            }

            var userId = TokenHelper.LayUserId(Request, _config);

            // ❌ FIX 2: Kiểm tra đã xác thực SĐT/Email chưa
            var userCheck = await _context.Users.FindAsync(userId);
            if (userCheck == null || !userCheck.DaXacThucSdt)
            {
                var returnUrl = $"/Booking/Create?khungGioId={khungGioId}&ngayThiDauStr={ngayThiDauStr}";
                // Giải phóng slot đang giữ
                var kgTam = await _context.KhungGios.FindAsync(khungGioId);
                if (kgTam != null && kgTam.TrangThai == "DangGiu")
                {
                    kgTam.TrangThai = "Trong";
                    kgTam.ThoiGianHetGiuCho = null;
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction("XacThuc", "Otp", new { returnUrl });
            }

            var khungGio = await _context.KhungGios
                .Include(k => k.SanBong)
                .FirstOrDefaultAsync(k => k.Id == khungGioId);
            if (khungGio == null) return NotFound();

            // ❌ FIX 1b: Nếu đặt hôm nay → kiểm tra khung giờ chưa qua
            if (ngayThiDau.Date == DateTime.Today)
            {
                var gioBD = khungGio.GioBatDau.ToTimeSpan();
                if (DateTime.Now.TimeOfDay >= gioBD)
                {
                    TempData["Error"] = "Khung giờ này đã qua hôm nay. Vui lòng chọn ngày khác!";
                    return RedirectToAction("Details", "Venues", new { id = khungGio.SanBongId });
                }
            }

            var tyLeCoc = khungGio.SanBong?.TyLeCoc ?? 0.30m;
            var tienCocGoc = khungGio.Gia * tyLeCoc;
            var tienCocSauGiam = tienCocGoc;

            // ── Áp dụng voucher nếu có ──────────────────────────
            UserVoucher? uvDung = null;
            if (!string.IsNullOrEmpty(userVoucherId))
            {
                uvDung = await _context.UserVouchers
                    .Include(uv => uv.Voucher)
                    .FirstOrDefaultAsync(uv => uv.MaSuDung == userVoucherId
                                            && uv.UserId == userId
                                            && !uv.IsUsed
                                            && uv.NgayHetHan > DateTime.Now);

                if (uvDung?.Voucher != null)
                {
                    var v = uvDung.Voucher;
                    if (v.LoaiGiam == "PhanTram")
                    {
                        var giam = tienCocGoc * (v.GiaTriGiam / 100m);
                        if (v.GiamToiDa.HasValue) giam = Math.Min(giam, v.GiamToiDa.Value);
                        tienCocSauGiam = Math.Max(0, tienCocGoc - giam);
                    }
                    else // SoTien
                    {
                        tienCocSauGiam = Math.Max(0, tienCocGoc - v.GiaTriGiam);
                    }
                }
            }

            // Sinh mã xác nhận theo format XN-YYYYMMDD-XXXX
            var maDatSan = $"XN-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";

            // ✅ FIX 3: Tính tổng tiền dịch vụ để cộng vào TongTien
            decimal tongTienDichVu = 0;
            var dichVuList = new List<(int dvId, int sl, decimal gia)>();
            if (dichVuIds != null)
            {
                for (int i = 0; i < dichVuIds.Count; i++)
                {
                    var sl = (soLuongs != null && i < soLuongs.Count) ? soLuongs[i] : 1;
                    if (sl <= 0) continue;
                    var dv = await _context.DichVus.FindAsync(dichVuIds[i]);
                    if (dv == null || !dv.IsActive) continue;
                    tongTienDichVu += dv.Gia * sl;
                    dichVuList.Add((dichVuIds[i], sl, dv.Gia));
                }
            }

            // TongTien = tiền sân + dịch vụ (cọc tính % trên tiền sân)
            var tongTienSan = khungGio.Gia + tongTienDichVu;

            // Tính lại cọc nếu cần (cọc tính trên tiền sân, không tính dịch vụ)
            // tienCocSauGiam đã tính đúng rồi

            var datSan = new DatSan
            {
                UserId = userId!.Value,
                KhungGioId = khungGioId,
                NgayThiDau = ngayThiDau,
                TienCoc = tienCocSauGiam,
                TongTien = tongTienSan,   // ← FIX: bao gồm cả dịch vụ
                MaXacNhan = maDatSan,
                TrangThai = "ChoDuyet",
                ThoiGianTao = DateTime.Now
            };
            await _datSanRepo.AddAsync(datSan);

            // Thêm dịch vụ vào đơn (KHÔNG trừ kho — chỉ trừ khi Staff check-in)
            foreach (var (dvId, sl, gia) in dichVuList)
            {
                _context.DatSanDichVus.Add(new DatSanDichVu
                {
                    DatSanId = datSan.Id,
                    DichVuId = dvId,
                    SoLuong = sl
                });
            }
            if (dichVuList.Any()) await _context.SaveChangesAsync();

            // ── Đánh dấu voucher đã dùng ────────────────────────
            if (uvDung != null)
            {
                uvDung.IsUsed = true;
                uvDung.NgaySuDung = DateTime.Now;
                uvDung.DatSanId = datSan.Id;
                await _context.SaveChangesAsync();

                // Ghi log điểm (trừ điểm đã ghi khi đổi, ở đây chỉ ghi lại note dùng voucher)
            }

            // Khoá slot
            khungGio.TrangThai = "DaDat";
            khungGio.ThoiGianHetGiuCho = null;
            await _khungGioRepo.UpdateAsync(khungGio);
            await _hub.Clients.Group($"san_{khungGio.SanBongId}")
                .SendAsync("CapNhatKhungGio", new { khungGioId = khungGio.Id, trangThai = "DaDat" });

            TempData["Success"] = $"Đặt sân thành công! Mã xác nhận: {maDatSan}. Vui lòng chờ Owner xác nhận.";
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

            // Đơn nào đã có đánh giá
            var daDanhGiaIds = await _context.DanhGias
                .Where(dg => dg.UserId == userId)
                .Select(dg => dg.DatSanId)
                .ToListAsync();
            ViewBag.DaDanhGiaIds = daDanhGiaIds.ToHashSet();

            // Tin matchmaking đang tìm
            var datSanIds = list.Select(d => d.Id).ToList();
            var daTim = await _context.Matchmakings
                .Where(m => m.TrangThai == "DangTim" && datSanIds.Contains(m.DatSanId))
                .ToListAsync();
            ViewBag.DaTim = daTim.Select(m => m.DatSanId).ToHashSet();
            ViewBag.MmIdMap = daTim.ToDictionary(m => m.DatSanId, m => m.Id);

            ViewBag.TrangThai = trangThai;
            return View(list);
        }

        // ══════════════════════════════════════════════════════════
        // POST /Booking/Huy
        // ══════════════════════════════════════════════════════════
        [HttpPost]
        [YeuCauDangNhap]
        public async Task<IActionResult> Huy(int id, string? lyDoHuy)
        {
            // ❌ FIX 4: Bắt buộc nhập lý do hủy
            if (string.IsNullOrWhiteSpace(lyDoHuy))
            {
                TempData["Error"] = "Vui lòng nhập lý do hủy đặt sân!";
                return RedirectToAction("MyBookings");
            }

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

            // ── Tính tiền hoàn theo đúng chính sách ─────────────
            decimal phanTramHoan = 1.0m; // mặc định 100%
            string thongBaoHoan;

            if (datSan.TrangThai == "DaXacNhan")
            {
                // Đã được Owner xác nhận → tính theo thời gian còn lại
                var gioBatDau = datSan.KhungGio?.GioBatDau.ToTimeSpan() ?? TimeSpan.Zero;
                var gioDauTran = datSan.NgayThiDau.Date.Add(gioBatDau);
                var conLai = gioDauTran - DateTime.Now;

                if (conLai.TotalHours >= 24)
                {
                    phanTramHoan = 1.0m;   // trước 24h: hoàn 100%
                    thongBaoHoan = "Hoàn 100% tiền cọc vì huỷ trước 24 giờ.";
                }
                else if (conLai.TotalHours >= 2)
                {
                    phanTramHoan = 0.5m;   // trong 24h: hoàn 50%
                    thongBaoHoan = "Hoàn 50% tiền cọc vì huỷ trong vòng 24 giờ.";
                }
                else
                {
                    phanTramHoan = 0m;     // trong 2h: không hoàn
                    thongBaoHoan = "Không hoàn cọc vì huỷ trong vòng 2 giờ trước trận.";
                }
            }
            else
            {
                // ChoDuyet → hoàn 100% (Owner chưa cam kết gì)
                thongBaoHoan = "Hoàn 100% tiền cọc vì Owner chưa xác nhận.";
            }

            var soTienHoan = Math.Round(datSan.TienCoc * phanTramHoan, 0);
            datSan.TrangThai = "DaHuy";
            datSan.GhiChuSuCo = $"Lý do hủy: {lyDoHuy}";  // Lưu lý do hủy
            datSan.KhungGio.TrangThai = "Trong";

            await _context.SaveChangesAsync();
            await _hub.Clients.Group($"san_{datSan.KhungGio.SanBongId}")
                .SendAsync("CapNhatKhungGio", new { khungGioId = datSan.KhungGioId, trangThai = "Trong" });

            TempData["Success"] = $"Đã huỷ đặt sân. {thongBaoHoan} Hoàn {soTienHoan:N0}đ trong 1–3 ngày làm việc.";
            return RedirectToAction("MyBookings");
        }

        // ══════════════════════════════════════════════════════════
        // POST /Booking/GuiKhieuNai
        // ══════════════════════════════════════════════════════════
        [HttpPost]
        [YeuCauDangNhap]
        public async Task<IActionResult> GuiKhieuNai(int datSanId, string lyDo)
        {
            var userId = TokenHelper.LayUserId(Request, _config);
            var don = await _context.DatSans
                .FirstOrDefaultAsync(d => d.Id == datSanId && d.UserId == userId);
            if (don == null) return NotFound();

            var daCoKN = await _context.KhieuNais
                .AnyAsync(k => k.DatSanId == datSanId && k.UserId == userId!.Value);
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

        // ══════════════════════════════════════════════════════════
        // GET /Booking/KhungGioTrong — AJAX endpoint
        // ══════════════════════════════════════════════════════════
        [YeuCauDangNhap]
        public async Task<IActionResult> KhungGioTrong(int sanId, string ngay, int khungGioHienTaiId)
        {
            if (!DateTime.TryParse(ngay, out var ngayParse)) ngayParse = DateTime.Today;
            var list = await _context.KhungGios
                .Where(k => k.SanBongId == sanId && k.TrangThai == "Trong" && k.Id != khungGioHienTaiId)
                .OrderBy(k => k.GioBatDau)
                .Select(k => new {
                    id = k.Id,
                    gioBatDau = k.GioBatDau.ToString(),
                    gioKetThuc = k.GioKetThuc.ToString(),
                    gia = k.Gia.ToString("N0")
                })
                .ToListAsync();
            return Json(list);
        }

        // ══════════════════════════════════════════════════════════
        // GET /Booking/YeuCauDoiGio/{datSanId}
        // ══════════════════════════════════════════════════════════
        [YeuCauDangNhap]
        public async Task<IActionResult> YeuCauDoiGio(int datSanId)
        {
            var userId = TokenHelper.LayUserId(Request, _config);
            var don = await _context.DatSans
                .Include(d => d.KhungGio).ThenInclude(k => k.SanBong)
                .FirstOrDefaultAsync(d => d.Id == datSanId && d.UserId == userId);

            if (don == null) return NotFound();
            if (don.TrangThai != "DaXacNhan")
            {
                TempData["Error"] = "Chỉ có thể yêu cầu đổi giờ với đơn đã xác nhận.";
                return RedirectToAction("MyBookings");
            }

            var daCo = await _context.YeuCauDoiGios
                .AnyAsync(y => y.DatSanId == datSanId && y.TrangThai == "ChoXuLy");
            if (daCo)
            {
                TempData["Error"] = "Đơn này đã có yêu cầu đổi giờ đang chờ xử lý.";
                return RedirectToAction("MyBookings");
            }

            var sanId = don.KhungGio.SanBongId;
            var khungGioTrong = await _context.KhungGios
                .Where(k => k.SanBongId == sanId && k.TrangThai == "Trong" && k.Id != don.KhungGioId)
                .OrderBy(k => k.GioBatDau)
                .ToListAsync();

            ViewBag.DatSan = don;
            ViewBag.KhungGioList = khungGioTrong;
            ViewBag.SanBong = don.KhungGio.SanBong;
            return View();
        }

        // ══════════════════════════════════════════════════════════
        // POST /Booking/GuiYeuCauDoiGio
        // ══════════════════════════════════════════════════════════
        [HttpPost]
        [YeuCauDangNhap]
        public async Task<IActionResult> GuiYeuCauDoiGio(
            int datSanId, int khungGioMoiId, string ngayMoiStr, string lyDo)
        {
            var userId = TokenHelper.LayUserId(Request, _config);
            var don = await _context.DatSans
                .Include(d => d.KhungGio).ThenInclude(k => k.SanBong)
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == datSanId && d.UserId == userId);

            if (don == null) return NotFound();
            if (don.TrangThai != "DaXacNhan")
            {
                TempData["Error"] = "Chỉ có thể yêu cầu đổi giờ với đơn đã xác nhận.";
                return RedirectToAction("MyBookings");
            }

            if (string.IsNullOrWhiteSpace(lyDo) || lyDo.Trim().Length < 20)
            {
                TempData["Error"] = "Lý do đổi giờ phải ít nhất 20 ký tự.";
                return RedirectToAction("YeuCauDoiGio", new { datSanId });
            }

            if (khungGioMoiId == don.KhungGioId)
            {
                TempData["Error"] = "Khung giờ mới phải khác khung giờ hiện tại.";
                return RedirectToAction("YeuCauDoiGio", new { datSanId });
            }

            if (!DateTime.TryParse(ngayMoiStr, out var ngayMoi))
                ngayMoi = don.NgayThiDau;

            var daCo = await _context.YeuCauDoiGios
                .AnyAsync(y => y.DatSanId == datSanId && y.TrangThai == "ChoXuLy");
            if (daCo)
            {
                TempData["Error"] = "Đơn này đã có yêu cầu đổi giờ đang chờ xử lý.";
                return RedirectToAction("MyBookings");
            }

            var yeuCau = new YeuCauDoiGio
            {
                DatSanId = datSanId,
                UserId = userId!.Value,
                KhungGioMoiId = khungGioMoiId,
                NgayMoi = ngayMoi,
                LyDo = lyDo.Trim(),
                TrangThai = "ChoXuLy",
                ThoiGianTao = DateTime.Now
            };
            _context.YeuCauDoiGios.Add(yeuCau);
            await _context.SaveChangesAsync();

            // Gửi email cho Staff của sân này
            var sanId = don.KhungGio.SanBongId;
            var staffList = await _context.StaffSanPhanCongs
                .Include(s => s.Staff)
                .Where(s => s.SanBongId == sanId)
                .ToListAsync();
            var kg = await _context.KhungGios.FindAsync(khungGioMoiId);
            var gioMoi = kg != null
                ? $"{kg.GioBatDau:hh\\:mm} – {kg.GioKetThuc:hh\\:mm}"
                : "N/A";
            foreach (var sp in staffList)
            {
                if (!string.IsNullOrEmpty(sp.Staff?.Email))
                    _ = Task.Run(() => _emailService.GuiEmailYeuCauMoiChoStaff(
                        sp.Staff.Email, sp.Staff.HoTen ?? "Staff",
                        don.User?.HoTen ?? "Khách", don.KhungGio.SanBong?.TenSan ?? "",
                        gioMoi, ngayMoi.ToString("dd/MM/yyyy"), lyDo.Trim()));
            }

            TempData["Success"] = "Đã gửi yêu cầu đổi khung giờ! Staff sẽ xem xét và phản hồi sớm.";
            return RedirectToAction("MyBookings");
        }

        // ══════════════════════════════════════════════════════════
        // GET /Booking/YeuCauDoiSan/{datSanId}
        // ══════════════════════════════════════════════════════════
        [YeuCauDangNhap]
        public async Task<IActionResult> YeuCauDoiSan(int datSanId)
        {
            var userId = TokenHelper.LayUserId(Request, _config);
            var don = await _context.DatSans
                .Include(d => d.KhungGio).ThenInclude(k => k.SanBong)
                .FirstOrDefaultAsync(d => d.Id == datSanId && d.UserId == userId);

            if (don == null) return NotFound();
            if (don.TrangThai != "DaXacNhan")
            {
                TempData["Error"] = "Chỉ có thể yêu cầu đổi sân với đơn đã xác nhận.";
                return RedirectToAction("MyBookings");
            }

            var daCo = await _context.YeuCauDoiSans
                .AnyAsync(y => y.DatSanId == datSanId && y.TrangThai == "ChoXuLy");
            if (daCo)
            {
                TempData["Error"] = "Đơn này đã có yêu cầu đổi sân đang chờ xử lý.";
                return RedirectToAction("MyBookings");
            }

            var loaiSan = don.KhungGio?.SanBong?.LoaiSan;
            var sanList = await _context.SanBongs
                .Where(s => s.TrangThaiDuyet == "DaDuyet"
                         && !s.IsHidden
                         && s.Id != don.KhungGio!.SanBongId
                         && (loaiSan == null || s.LoaiSan == loaiSan))
                .OrderBy(s => s.TenSan)
                .ToListAsync();

            ViewBag.DatSan = don;
            ViewBag.SanList = sanList;
            ViewBag.NgayThiDau = don.NgayThiDau.ToString("yyyy-MM-dd");
            return View();
        }

        // ══════════════════════════════════════════════════════════
        // GET /Booking/KhungGioTrongCuaSan — AJAX endpoint
        // ══════════════════════════════════════════════════════════
        [YeuCauDangNhap]
        public async Task<IActionResult> KhungGioTrongCuaSan(int sanId, string? ngay)
        {
            var list = await _context.KhungGios
                .Where(k => k.SanBongId == sanId && k.TrangThai == "Trong")
                .OrderBy(k => k.GioBatDau)
                .Select(k => new {
                    id = k.Id,
                    gioBatDau = k.GioBatDau.ToString(),
                    gioKetThuc = k.GioKetThuc.ToString(),
                    gia = (double)k.Gia,
                    giaFmt = k.Gia.ToString("N0")
                })
                .ToListAsync();
            return Json(list);
        }

        // ══════════════════════════════════════════════════════════
        // POST /Booking/GuiYeuCauDoiSan
        // ══════════════════════════════════════════════════════════
        [HttpPost]
        [YeuCauDangNhap]
        public async Task<IActionResult> GuiYeuCauDoiSan(
            int datSanId, int sanMoiId, int khungGioMoiId, string lyDo)
        {
            var userId = TokenHelper.LayUserId(Request, _config);
            var don = await _context.DatSans
                .Include(d => d.KhungGio).ThenInclude(k => k.SanBong)
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == datSanId && d.UserId == userId);

            if (don == null) return NotFound();
            if (don.TrangThai != "DaXacNhan")
            {
                TempData["Error"] = "Chỉ có thể yêu cầu đổi sân với đơn đã xác nhận.";
                return RedirectToAction("MyBookings");
            }

            if (string.IsNullOrWhiteSpace(lyDo) || lyDo.Trim().Length < 10)
            {
                TempData["Error"] = "Lý do đổi sân phải ít nhất 10 ký tự.";
                return RedirectToAction("YeuCauDoiSan", new { datSanId });
            }

            var kgMoi = await _context.KhungGios
                .Include(k => k.SanBong)
                .FirstOrDefaultAsync(k => k.Id == khungGioMoiId && k.SanBongId == sanMoiId);

            if (kgMoi == null || kgMoi.TrangThai != "Trong")
            {
                TempData["Error"] = "Khung giờ đã chọn không còn trống. Vui lòng chọn lại.";
                return RedirectToAction("YeuCauDoiSan", new { datSanId });
            }

            var daCo = await _context.YeuCauDoiSans
                .AnyAsync(y => y.DatSanId == datSanId && y.TrangThai == "ChoXuLy");
            if (daCo)
            {
                TempData["Error"] = "Đơn này đã có yêu cầu đổi sân đang chờ xử lý.";
                return RedirectToAction("MyBookings");
            }

            var chenhLech = kgMoi.Gia - (don.KhungGio?.Gia ?? 0);

            var yeuCau = new YeuCauDoiSan
            {
                DatSanId = datSanId,
                UserId = userId!.Value,
                SanMoiId = sanMoiId,
                KhungGioMoiId = khungGioMoiId,
                NgayThiDau = don.NgayThiDau,
                LyDo = lyDo.Trim(),
                TrangThai = "ChoXuLy",
                ChenhLechGia = chenhLech,
                ThoiGianTao = DateTime.Now
            };
            _context.YeuCauDoiSans.Add(yeuCau);
            await _context.SaveChangesAsync();

            // Gửi email Owner sân mới
            var sanMoi = kgMoi.SanBong;
            if (sanMoi != null)
            {
                var owner = await _context.Users.FindAsync(sanMoi.OwnerId);
                if (owner != null && !string.IsNullOrEmpty(owner.Email))
                {
                    var gioMoi = $"{kgMoi.GioBatDau:hh\\:mm} – {kgMoi.GioKetThuc:hh\\:mm}";
                    _ = Task.Run(() => _emailService.GuiEmailYeuCauDoiSanChoOwner(
                        owner.Email, owner.HoTen ?? "Owner",
                        don.User?.HoTen ?? "Khách",
                        don.KhungGio?.SanBong?.TenSan ?? "",
                        sanMoi.TenSan, gioMoi,
                        don.NgayThiDau.ToString("dd/MM/yyyy"),
                        lyDo.Trim(), chenhLech));
                }
            }

            TempData["Success"] = "Đã gửi yêu cầu đổi sân! Owner sân mới sẽ xem xét và phản hồi sớm.";
            return RedirectToAction("MyBookings");
        }
    }
}