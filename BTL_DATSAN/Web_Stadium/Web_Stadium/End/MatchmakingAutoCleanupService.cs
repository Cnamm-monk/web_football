using Microsoft.EntityFrameworkCore;
using Web_Stadium.EFCore;

namespace Web_Stadium.End
{
    /// <summary>
    /// Background Service chay ngam moi 5 phut.
    /// Tu dong huy tin Matchmaking truoc 2 gio neu khong co doi lien he.
    /// Dang ky: Program.cs -> builder.Services.AddHostedService<MatchmakingAutoCleanupService>();
    /// </summary>
    public class MatchmakingAutoCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MatchmakingAutoCleanupService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);
        private const int GIO_TRUOC_HUY = 2;

        public MatchmakingAutoCleanupService(
            IServiceScopeFactory scopeFactory,
            ILogger<MatchmakingAutoCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[Matchmaking] Auto-cleanup service da khoi dong");
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try { await HuyTinHetHan(stoppingToken); }
                catch (Exception ex) when (ex is not OperationCanceledException)
                { _logger.LogError(ex, "[Matchmaking] Loi cleanup"); }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task HuyTinHetHan(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SanBongContext>();

            var nguong = DateTime.Now.AddHours(GIO_TRUOC_HUY);

            var tatCaTin = await db.Matchmakings
                .Include(m => m.DatSan)
                    .ThenInclude(d => d!.KhungGio)
                .Where(m => m.TrangThai == "DangTim")
                .ToListAsync(ct);

            var canHuy = tatCaTin.Where(m =>
            {
                if (m.DatSan?.KhungGio == null) return false;
                // NgayThiDau la DateTime, GioBatDau la TimeOnly
                // Ghep ngay tu NgayThiDau + gio tu GioBatDau
                var ngay = m.DatSan.NgayThiDau.Date;
                var gio = m.DatSan.KhungGio.GioBatDau;
                var thoiGianBD = new DateTime(
                    ngay.Year, ngay.Month, ngay.Day,
                    gio.Hour, gio.Minute, gio.Second);
                // Huy khi con duoi 2h va chua dien ra
                return thoiGianBD <= nguong && thoiGianBD > DateTime.Now;
            }).ToList();

            if (!canHuy.Any()) return;

            foreach (var tin in canHuy)
            {
                tin.TrangThai = "TuDongHuy";
                tin.LyDoHuy = $"Tự động hủy: không có đội liên hệ trước {GIO_TRUOC_HUY} giờ thi đấu";
                _logger.LogInformation("[Matchmaking] Huy tin #{id}: {tieude}", tin.Id, tin.TieuDe);
            }

            await db.SaveChangesAsync(ct);
            _logger.LogInformation("[Matchmaking] Da huy {count} tin", canHuy.Count);
        }
    }
}