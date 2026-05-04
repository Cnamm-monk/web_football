using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_Stadium.End;
using Web_Stadium.EFCore;


namespace Web_Stadium.Controllers
{
    // Xem danh sách sân, tìmm kiếm/lọc sân, xem chi tiết sân
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

        // GET: /Venues - Danh sách sân + tìm kiếm  
        public async Task<IActionResult> Index(
            string? quan, string? loaiSan,
            string? loaiCo, string? tuKhoa)
        {
            var query = _context.SanBongs
                .Where(s =>
                    s.TrangThaiDuyet == "DaDuyet");

            // Lọc theo quận
            if (!string.IsNullOrEmpty(quan))
                query = query.Where(
                    s => s.Quan == quan);

            // Lọc theo sân 5/7/11
            if (!string.IsNullOrEmpty(loaiSan))
            {
                query = query.Where(
                    s => s.LoaiSan == loaiSan);
            }

            // Lọc theo loại cỏ
            if (!string.IsNullOrEmpty(loaiCo))
            {
                query = query.Where(
                    s => s.LoaiCo == loaiCo);
            }

            // Tìm kiếm theo tên sân hoặc địa chỉ
            if (!string.IsNullOrEmpty(tuKhoa))
            {
                query = query.Where(
                    s => s.TenSan.Contains(tuKhoa) ||
                         s.DiaChi.Contains(tuKhoa) ||
                         s.Quan.Contains(tuKhoa));
            }

            //Sắp xếp theo điểm đánh giá 
            var danhSach = await query
                .OrderByDescending(
                s => s.DanhGiaTrungBinh)
                .ToListAsync();

            //Truyền bộ lọc hiện tại xuống View
            ViewBag.Quan = quan;
            ViewBag.LoaiSan = loaiSan;
            ViewBag.LoaiCo = loaiCo;
            ViewBag.TuKhoa = tuKhoa;

            return View(danhSach);
        }

        // GET: /Venues/Details/5 - Xem chi tiết sân
        public async Task<IActionResult> Details(int id)
        {
            var sanBong = await _context.SanBongs
                .Include (s => s.KhungGios)
                .Include (s => s.DanhGias)
                    .ThenInclude(d => d.User)
                    .FirstOrDefaultAsync(s => s.Id == id);
            if (sanBong == null)
                return NotFound();

            //Tính điểm đánh giá trung bình
            if (sanBong.DanhGias.Any())
            {
                sanBong.DanhGiaTrungBinh =
                    sanBong.DanhGias
                        .Average(d => d.SoSao);
                await _context.SaveChangesAsync();
            }

            //Truyền danh sach dịch vụ xuống View
            ViewBag.DichVus = await _context.DichVus.ToListAsync();

            return View(sanBong);
        }

        // POST: /Venues/Details/5 - Gửi đánh giá sân
        [HttpPost]
        public async Task<IActionResult> DanhGiaSan(
            int sanBongId, int soSao, string? nhanXet)
        {
            var userId = TokenHelper.LayUserId(
                 Request, _config);
            if (userId == null)
                return RedirectToAction("Login", "Auth");
            var danhGia = new DanhGia
            {
                UserId = userId.Value,
                SanBongId = sanBongId,
                SoSao = soSao,
                NhanXet = nhanXet,
                NgayDanhGia = DateTime.UtcNow
            };
            _context.DanhGias.Add(danhGia);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = sanBongId });
        }
    }
}
