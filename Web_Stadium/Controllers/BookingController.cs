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
            int? voucherSanId,
            int? voucherHeThongId)
        {
            if (!DateTime.TryParse(ngayThiDauStr, out var ngayThiDau) || ngayThiDau < new DateTime(1753, 1, 1))
                ngayThiDau = DateTime.Today;

            if (ngayThiDau.Date < DateTime.Today)
            {
                TempData["Error"] = "Không thể đặt sân cho ngày đã qua. Vui lòng chọn ngày hôm nay hoặc tương lai!";
                return RedirectToAction("Details", "Venues", new { id = khungGioId });
            }

            var userId = TokenHelper.LayUserId(Request, _config);

            var userCheck = await _context.Users.FindAsync(userId);
            if (userCheck == null || !userCheck.DaXacThucSdt)
            {
                var returnUrl = $"/Booking/Create?khungGioId={khungGioId}&ngayThiDauStr={ngayThiDauStr}";
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

            if (ngayThiDau.Date == DateTime.Today)
            {
                var gioBD = khungGio.GioBatDau.ToTimeSpan();
                if (DateTime.Now.TimeOfDay >= gioBD)
                {
                    TempData["Error"] = "Khung giờ này đã qua hôm nay. Vui lòng chọn ngày khác!";
                    return RedirectToAction("Details", "Venues", new { id = khungGio.SanBongId });
                }
            }

            // ── Tính tiền dịch vụ ──────────────────────────────
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

            // ── Logic tiền: giảm Owner trước, rồi giảm HeThong, rồi tính cọc ──
            decimal giaGoc = khungGio.Gia + tongTienDichVu;
            decimal tienGiamSan = 0;
            decimal tienGiamHeThong = 0;
            int? voucherSanApDungId = null;
            int? voucherHeThongApDungId = null;
            var now = DateTime.Now;

            // Áp voucher sân (Owner) — tính trên giá gốc
            if (voucherSanId.HasValue)
            {
                var v = await _context.Vouchers.FindAsync(voucherSanId.Value);
                if (v != null && v.IsActive && v.LoaiVoucher == "Owner"
                    && now >= v.NgayBatDau && now <= v.NgayHetHan
                    && (v.SoLuong == 0 || v.DaDung < v.SoLuong)
                    && giaGoc >= v.DieuKienToiThieu)
                {
                    tienGiamSan = v.LoaiGiam == "PhanTram"
                        ? giaGoc * v.GiaTriGiam / 100m
                        : Math.Min(v.GiaTriGiam, giaGoc);
                    if (v.LoaiGiam == "PhanTram" && v.GiamToiDa.HasValue)
                        tienGiamSan = Math.Min(tienGiamSan, v.GiamToiDa.Value);
                    v.DaDung++;
                    voucherSanApDungId = v.Id;
                }
            }

            // Áp voucher hệ thống (HeThong) — tính trên giá sau khi đã giảm sân
            decimal sauGiamSan = Math.Max(0, giaGoc - tienGiamSan);
            if (voucherHeThongId.HasValue)
            {
                var v = await _context.Vouchers.FindAsync(voucherHeThongId.Value);
                if (v != null && v.IsActive && v.LoaiVoucher == "HeThong"
                    && now >= v.NgayBatDau && now <= v.NgayHetHan
                    && (v.SoLuong == 0 || v.DaDung < v.SoLuong)
                    && sauGiamSan >= v.DieuKienToiThieu)
                {
                    tienGiamHeThong = v.LoaiGiam == "PhanTram"
                        ? sauGiamSan * v.GiaTriGiam / 100m
                        : Math.Min(v.GiaTriGiam, sauGiamSan);
                    if (v.LoaiGiam == "PhanTram" && v.GiamToiDa.HasValue)
                        tienGiamHeThong = Math.Min(tienGiamHeThong, v.GiamToiDa.Value);
                    v.DaDung++;
                    voucherHeThongApDungId = v.Id;
                }
            }

            decimal tongSauGiam = Math.Max(0, giaGoc - tienGiamSan - tienGiamHeThong);
            var tyLeCoc = khungGio.SanBong?.TyLeCoc ?? 0.30m;
            decimal tienCoc = tongSauGiam * tyLeCoc;

            var maDatSan = $"XN-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";

            var datSan = new DatSan
            {
                UserId = userId!.Value,
                KhungGioId = khungGioId,
                NgayThiDau = ngayThiDau,
                TienGoc = giaGoc,
                TienGiamSan = tienGiamSan,
                TienGiamHeThong = tienGiamHeThong,
                TienCoc = tienCoc,
                TongTien = tongSauGiam,
                VoucherSanId = voucherSanApDungId,
                VoucherHeThongId = voucherHeThongApDungId,
                MaXacNhan = maDatSan,
                TrangThai = "ChoDuyet",
                ThoiGianTao = DateTime.Now
            };
            await _datSanRepo.AddAsync(datSan);

            foreach (var (dvId, sl, gia) in dichVuList)
            {
                _context.DatSanDichVus.Add(new DatSanDichVu
                {
                    DatSanId = datSan.Id,
                    DichVuId = dvId,
                    SoLuong = sl
                });
            }
            if (dichVuList.Any() || voucherSanApDungId.HasValue || voucherHeThongApDungId.HasValue)
                await _context.SaveChangesAsync();

            // Khoá slot
            khungGio.TrangThai = "DaDat";
            khungGio.ThoiGianHetGiuCho = null;
            await _khungGioRepo.UpdateAsync(khungGio);
            await _hub.Clients.Group($"san_{khungGio.SanBongId}")
                .SendAsync("CapNhatKhungGio", new { khungGioId = khungGio.Id, trangThai = "DaDat" });

            var tongGiam = tienGiamSan + tienGiamHeThong;
            var msgGiam = tongGiam > 0 ? $" (Đã giảm {tongGiam:N0}đ từ voucher)" : "";
            TempData["Success"] = $"Đặt sân thành công! Mã: {maDatSan}. Tiền cọc: {tienCoc:N0}đ{msgGiam}. Vui lòng chờ Owner xác nhận.";
            return RedirectToAction("MyBookings");
        }

        // ══════════════════════════════════════════════════════════
        // GET /Booking/LayVoucher?sanBongId=1&tongTien=200000
        // ══════════════════════════════════════════════════════════
        [HttpGet]
        [YeuCauDangNhap]
        public async Task<IActionResult> LayVoucher(int sanBongId, decimal tongTien)
        {
            var now = DateTime.Now;

            var rawSan = await _context.Vouchers
                .Where(v => v.LoaiVoucher == "Owner"
                         && v.SanBongId == sanBongId
                         && v.IsActive
                         && v.NgayBatDau <= now
                         && v.NgayHetHan >= now
                         && (v.SoLuong == 0 || v.DaDung < v.SoLuong)
                         && tongTien >= v.DieuKienToiThieu)
                .ToListAsync();

            var rawHT = await _context.Vouchers
                .Where(v => v.LoaiVoucher == "HeThong"
                         && v.IsActive
                         && v.NgayBatDau <= now
                         && v.NgayHetHan >= now
                         && (v.SoLuong == 0 || v.DaDung < v.SoLuong)
                         && tongTien >= v.DieuKienToiThieu)
                .ToListAsync();

            decimal TinhGiam(Voucher v, decimal gia)
            {
                var g = v.LoaiGiam == "PhanTram" ? gia * v.GiaTriGiam / 100m : Math.Min(v.GiaTriGiam, gia);
                if (v.LoaiGiam == "PhanTram" && v.GiamToiDa.HasValue) g = Math.Min(g, v.GiamToiDa.Value);
                return g;
            }

            var voucherSan = rawSan
                .Select(v => new
                {
                    v.Id, v.TenVoucher, v.MoTa, v.LoaiGiam,
                    v.GiaTriGiam, v.GiamToiDa, v.NgayHetHan,
                    v.DieuKienToiThieu,
                    conLai = v.SoLuong == 0 ? -1 : v.SoLuong - v.DaDung,
                    soTienGiam = TinhGiam(v, tongTien),
                    conNgay = (int)(v.NgayHetHan - now).TotalDays
                })
                .OrderByDescending(x => x.soTienGiam)
                .ToList();

            var voucherHeThong = rawHT
                .Select(v => new
                {
                    v.Id, v.TenVoucher, v.MoTa, v.LoaiGiam,
                    v.GiaTriGiam, v.GiamToiDa, v.NgayHetHan,
                    v.DieuKienToiThieu,
                    conLai = v.SoLuong == 0 ? -1 : v.SoLuong - v.DaDung,
                    soTienGiam = TinhGiam(v, tongTien),
                    conNgay = (int)(v.NgayHetHan - now).TotalDays
                })
                .OrderByDescending(x => x.soTienGiam)
                .ToList();

            return Json(new { voucherSan, voucherHeThong });
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
    }
}