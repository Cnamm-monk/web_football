using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_Stadium.EFCore;
using Web_Stadium.EFCore;
using Web_Stadium.Filters;

namespace Web_Stadium.Controllers
{
    [YeuCauDangNhap("Staff")]
    public class StaffController : Controller
    {
        private readonly SanBongContext _context;
        private readonly IConfiguration _config;

        public StaffController(SanBongContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        private int GetStaffId() => TokenHelper.LayUserId(Request, _config)!.Value;

        // Lấy danh sách SanBongId mà Staff này được phân công
        private async Task<List<int>> GetSanDuocGiaoAsync()
        {
            var staffId = GetStaffId();
            return await _context.StaffSanPhanCongs
                .Where(p => p.StaffId == staffId)
                .Select(p => p.SanBongId)
                .ToListAsync();
        }

        // ══════════════════════════════════════════════════════════
        // 1. DASHBOARD CA TRỰC — Lịch đặt sân hôm nay
        // ══════════════════════════════════════════════════════════
        public async Task<IActionResult> Index()
        {
            var sanIds = await GetSanDuocGiaoAsync();
            var now = DateTime.Now;

            // Đơn hôm nay của sân được phân công — sắp xếp theo giờ
            var donHomNay = await _context.DatSans
                .Include(d => d.User)
                .Include(d => d.KhungGio).ThenInclude(k => k.SanBong)
                .Include(d => d.DatSanDichVus).ThenInclude(dv => dv.DichVu)
                .Where(d => sanIds.Contains(d.KhungGio.SanBongId)
                         && d.NgayThiDau.Date == now.Date
                         && d.TrangThai != "DaHuy")
                .OrderBy(d => d.KhungGio.GioBatDau)
                .ToListAsync();

            // Đơn DangSuDung — đang diễn ra
            ViewBag.DangDien = donHomNay.Where(d => d.TrangThai == "DangSuDung").ToList();
            // Đơn DaXacNhan — sắp đến
            ViewBag.SapDen = donHomNay.Where(d => d.TrangThai == "DaXacNhan").ToList();
            // Đơn HoanThanh — đã xong hôm nay
            ViewBag.HoanThanh = donHomNay.Where(d => d.TrangThai == "HoanThanh").ToList();

            // Sân được phân công
            ViewBag.DanhSachSan = await _context.SanBongs
                .Where(s => sanIds.Contains(s.Id))
                .ToListAsync();

            ViewBag.SanIds = sanIds;
            ViewBag.Now = now;

            return View(donHomNay);
        }

        // ══════════════════════════════════════════════════════════
        // 2. CHECK-IN — Tra cứu + chuyển DaXacNhan → DangSuDung
        // ══════════════════════════════════════════════════════════
        public async Task<IActionResult> CheckIn(string? tuKhoa)
        {
            var sanIds = await GetSanDuocGiaoAsync();
            ViewBag.TuKhoa = tuKhoa;
            ViewBag.KetQua = null;

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                // Tìm theo MaXacNhan hoặc SoDienThoai của User
                var don = await _context.DatSans
                    .Include(d => d.User)
                    .Include(d => d.KhungGio).ThenInclude(k => k.SanBong)
                    .Include(d => d.DatSanDichVus).ThenInclude(dv => dv.DichVu)
                    .Where(d => sanIds.Contains(d.KhungGio.SanBongId)
                             && (d.MaXacNhan == tuKhoa.Trim()
                              || d.User.SoDienThoai == tuKhoa.Trim()))
                    .OrderByDescending(d => d.NgayThiDau)
                    .FirstOrDefaultAsync();

                ViewBag.KetQua = don;
                if (don == null)
                    ViewBag.ThongBao = "Không tìm thấy đơn với mã/SĐT này tại sân của bạn.";
            }

            return View();
        }

        // POST: Thực hiện check-in
        [HttpPost]
        public async Task<IActionResult> ThucHienCheckIn(int datSanId)
        {
            var sanIds = await GetSanDuocGiaoAsync();
            var don = await _context.DatSans
                .Include(d => d.KhungGio)
                .FirstOrDefaultAsync(d => d.Id == datSanId
                                       && sanIds.Contains(d.KhungGio.SanBongId));

            if (don == null) return NotFound();

            if (don.TrangThai != "DaXacNhan")
            {
                TempData["Error"] = $"Đơn #{datSanId} đang ở trạng thái \"{don.TrangThai}\", không thể check-in.";
                return RedirectToAction("CheckIn");
            }

            don.TrangThai = "DangSuDung";
            don.StaffCheckInId = GetStaffId();
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Check-in thành công! Đơn #{don.MaXacNhan}";
            return RedirectToAction("Index");
        }

        // ══════════════════════════════════════════════════════════
        // 3. POS MINI — Thêm dịch vụ vào đơn đang DangSuDung
        // ══════════════════════════════════════════════════════════
        public async Task<IActionResult> POS(int datSanId)
        {
            var sanIds = await GetSanDuocGiaoAsync();
            var don = await _context.DatSans
                .Include(d => d.User)
                .Include(d => d.KhungGio).ThenInclude(k => k.SanBong)
                .Include(d => d.DatSanDichVus).ThenInclude(dv => dv.DichVu)
                .FirstOrDefaultAsync(d => d.Id == datSanId
                                       && sanIds.Contains(d.KhungGio.SanBongId));

            if (don == null) return NotFound();

            // Danh sách dịch vụ của sân đó còn tồn kho
            ViewBag.DanhSachDichVu = await _context.DichVus
                .Where(dv => dv.SanBongId == don.KhungGio.SanBongId
                          && dv.IsActive && dv.TonKho > 0)
                .ToListAsync();

            return View(don);
        }

        // POST: Thêm dịch vụ vào đơn + trừ tồn kho
        [HttpPost]
        public async Task<IActionResult> ThemDichVuPOS(int datSanId, int dichVuId, int soLuong)
        {
            var sanIds = await GetSanDuocGiaoAsync();
            var don = await _context.DatSans
                .Include(d => d.KhungGio)
                .FirstOrDefaultAsync(d => d.Id == datSanId
                                       && sanIds.Contains(d.KhungGio.SanBongId));
            var dv = await _context.DichVus.FindAsync(dichVuId);

            if (don == null || dv == null) return NotFound();

            if (don.TrangThai != "DangSuDung")
            {
                TempData["Error"] = "Chỉ thêm dịch vụ khi đơn đang ở trạng thái Đang sử dụng!";
                return RedirectToAction("POS", new { datSanId });
            }
            if (dv.TonKho < soLuong)
            {
                TempData["Error"] = $"Tồn kho \"{dv.TenDichVu}\" không đủ! Còn {dv.TonKho} đơn vị.";
                return RedirectToAction("POS", new { datSanId });
            }

            // Kiểm tra dịch vụ đã có trong đơn chưa — nếu có thì cộng thêm
            var daCoTrongDon = await _context.DatSanDichVus
                .FirstOrDefaultAsync(x => x.DatSanId == datSanId && x.DichVuId == dichVuId);

            if (daCoTrongDon != null)
                daCoTrongDon.SoLuong += soLuong;
            else
                _context.DatSanDichVus.Add(new DatSanDichVu
                {
                    DatSanId = datSanId,
                    DichVuId = dichVuId,
                    SoLuong = soLuong
                });

            // Trừ tồn kho
            dv.TonKho -= soLuong;

            // Cộng vào TongTien của đơn
            don.TongTien += dv.Gia * soLuong;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã thêm {soLuong}x \"{dv.TenDichVu}\" vào đơn!";
            return RedirectToAction("POS", new { datSanId });
        }

        // ══════════════════════════════════════════════════════════
        // 4. CHECK-OUT — Thu tiền + chuyển DangSuDung → HoanThanh
        // ══════════════════════════════════════════════════════════
        public async Task<IActionResult> CheckOut(int datSanId)
        {
            var sanIds = await GetSanDuocGiaoAsync();
            var don = await _context.DatSans
                .Include(d => d.User)
                .Include(d => d.KhungGio).ThenInclude(k => k.SanBong)
                .Include(d => d.DatSanDichVus).ThenInclude(dv => dv.DichVu)
                .FirstOrDefaultAsync(d => d.Id == datSanId
                                       && sanIds.Contains(d.KhungGio.SanBongId));

            if (don == null) return NotFound();

            // Tính tiền còn lại cần thu
            var tongTienDichVu = don.DatSanDichVus.Sum(x => x.DichVu.Gia * x.SoLuong);
            var tongTatCa = don.KhungGio.Gia + tongTienDichVu;
            ViewBag.TongTienDichVu = tongTienDichVu;
            ViewBag.TongTatCa = tongTatCa;
            ViewBag.ConLai = tongTatCa - don.TienCoc;

            return View(don);
        }

        // POST: Thực hiện check-out
        [HttpPost]
        public async Task<IActionResult> ThucHienCheckOut(int datSanId)
        {
            var sanIds = await GetSanDuocGiaoAsync();
            var don = await _context.DatSans
                .Include(d => d.KhungGio).ThenInclude(k => k.SanBong)
                .Include(d => d.DatSanDichVus).ThenInclude(dv => dv.DichVu)
                .FirstOrDefaultAsync(d => d.Id == datSanId
                                       && sanIds.Contains(d.KhungGio.SanBongId));

            if (don == null) return NotFound();

            if (don.TrangThai != "DangSuDung")
            {
                TempData["Error"] = $"Đơn #{datSanId} không ở trạng thái Đang sử dụng!";
                return RedirectToAction("Index");
            }

            var tongTienDichVu = don.DatSanDichVus.Sum(x => x.DichVu.Gia * x.SoLuong);
            var tongTatCa = don.KhungGio.Gia + tongTienDichVu;

            don.TrangThai = "HoanThanh";
            don.TongTien = tongTatCa;
            don.StaffCheckOutId = GetStaffId();

            // Giải phóng slot
            var kg = don.KhungGio;
            kg.TrangThai = "Trong";

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Check-out thành công! Thu {(tongTatCa - don.TienCoc):N0}đ. Tổng đơn: {tongTatCa:N0}đ.";
            return RedirectToAction("Index");
        }

        // ══════════════════════════════════════════════════════════
        // 5. BÁO CÁO SỰ CỐ — No-show / Hỏng hóc
        // ══════════════════════════════════════════════════════════
        public async Task<IActionResult> SuCo()
        {
            var sanIds = await GetSanDuocGiaoAsync();
            // Danh sách đơn có thể báo sự cố (DaXacNhan hoặc DangSuDung)
            ViewBag.DonCoThe = await _context.DatSans
                .Include(d => d.User)
                .Include(d => d.KhungGio).ThenInclude(k => k.SanBong)
                .Where(d => sanIds.Contains(d.KhungGio.SanBongId)
                         && (d.TrangThai == "DaXacNhan" || d.TrangThai == "DangSuDung")
                         && d.LoaiSuCo == null)
                .OrderByDescending(d => d.NgayThiDau)
                .Take(20).ToListAsync();

            // Lịch sử sự cố gần đây của sân mình
            ViewBag.LichSuSuCo = await _context.DatSans
                .Include(d => d.User)
                .Include(d => d.KhungGio).ThenInclude(k => k.SanBong)
                .Where(d => sanIds.Contains(d.KhungGio.SanBongId)
                         && d.LoaiSuCo != null)
                .OrderByDescending(d => d.ThoiGianTao)
                .Take(10).ToListAsync();

            return View();
        }

        // POST: Ghi nhận sự cố
        [HttpPost]
        public async Task<IActionResult> GhiNhanSuCo(int datSanId, string loaiSuCo, string ghiChu)
        {
            var sanIds = await GetSanDuocGiaoAsync();
            var don = await _context.DatSans
                .Include(d => d.KhungGio)
                .FirstOrDefaultAsync(d => d.Id == datSanId
                                       && sanIds.Contains(d.KhungGio.SanBongId));

            if (don == null) return NotFound();

            don.LoaiSuCo = loaiSuCo;  // "NoShow" | "HongHoc"
            don.GhiChuSuCo = ghiChu;

            // No-show: khách không đến → chuyển về DaHuy để Owner xem xét
            if (loaiSuCo == "NoShow" && don.TrangThai == "DaXacNhan")
                don.TrangThai = "DaHuy";

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã ghi nhận sự cố \"{loaiSuCo}\" cho đơn #{don.MaXacNhan}. Owner sẽ được thông báo.";
            return RedirectToAction("SuCo");
        }

        // ══════════════════════════════════════════════════════════
        // AJAX: Lấy trạng thái slot realtime (dùng cho dashboard)
        // ══════════════════════════════════════════════════════════
        [HttpGet]
        public async Task<IActionResult> GetTrangThaiSlots()
        {
            var sanIds = await GetSanDuocGiaoAsync();
            var slots = await _context.KhungGios
                .Include(k => k.SanBong)
                .Where(k => sanIds.Contains(k.SanBongId))
                .Select(k => new
                {
                    id = k.Id,
                    sanTen = k.SanBong.TenSan,
                    bat = k.GioBatDau.ToString(@"hh\:mm"),
                    ket = k.GioKetThuc.ToString(@"hh\:mm"),
                    trangThai = k.TrangThai
                }).ToListAsync();
            return Json(slots);
        }
    }
}