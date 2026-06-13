using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_Stadium.EFCore;

namespace Web_Stadium.Controllers
{
    public class SearchController : Controller
    {
        private readonly SanBongContext _context;

        public SearchController(SanBongContext context)
        {
            _context = context;
        }

        // GET /Search/Global?q=keyword
        public async Task<IActionResult> Global(string? q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
                return Json(new { sanBongs = Array.Empty<object>(), giaiDaus = Array.Empty<object>(), matchmakings = Array.Empty<object>(), chuyenNhuongs = Array.Empty<object>() });

            q = q.Trim();

            var sanTask = _context.SanBongs
                .Where(s => s.TrangThaiDuyet == "DaDuyet" && !s.IsHidden
                    && (s.TenSan.Contains(q) || s.DiaChi.Contains(q) || s.Quan.Contains(q)))
                .Take(4)
                .Select(s => new {
                    id = s.Id,
                    tenSan = s.TenSan,
                    diaChi = s.DiaChi,
                    quan = s.Quan,
                    danhGia = s.DanhGia.Any()
                        ? s.DanhGia.Average(d => (double)d.SoSao)
                        : (double?)null
                })
                .ToListAsync();

            var giaiTask = _context.GiaiDaus
                .Where(g => g.TrangThai != "Draft" && g.TenGiai.Contains(q))
                .OrderByDescending(g => g.NgayBatDau)
                .Take(3)
                .Select(g => new {
                    id = g.Id,
                    tenGiai = g.TenGiai,
                    trangThai = g.TrangThai,
                    ngayBatDau = g.NgayBatDau
                })
                .ToListAsync();

            var mmTask = _context.Matchmakings
                .Include(m => m.DatSan).ThenInclude(d => d.KhungGio).ThenInclude(k => k.SanBong)
                .Where(m => m.TrangThai == "DangTim"
                    && (m.TieuDe.Contains(q) || m.DatSan.KhungGio.SanBong.TenSan.Contains(q)))
                .Take(3)
                .Select(m => new {
                    id = m.Id,
                    tieuDe = m.TieuDe,
                    tenSan = m.DatSan.KhungGio.SanBong.TenSan,
                    ngayThiDau = m.DatSan.NgayThiDau
                })
                .ToListAsync();

            var cnTask = _context.ChuyenNhuongs
                .Include(c => c.DatSan).ThenInclude(d => d.KhungGio).ThenInclude(k => k.SanBong)
                .Where(c => c.TrangThai == "DangTim"
                    && (c.TieuDe.Contains(q) || c.DatSan.KhungGio.SanBong.TenSan.Contains(q)))
                .Take(3)
                .Select(c => new {
                    id = c.Id,
                    tieuDe = c.TieuDe,
                    tenSan = c.DatSan.KhungGio.SanBong.TenSan,
                    giaChuyenNhuong = c.GiaChuyenNhuong
                })
                .ToListAsync();

            await Task.WhenAll(sanTask, giaiTask, mmTask, cnTask);

            return Json(new {
                sanBongs = sanTask.Result,
                giaiDaus = giaiTask.Result,
                matchmakings = mmTask.Result,
                chuyenNhuongs = cnTask.Result
            });
        }
    }
}
