using Web_Stadium.EFCore;

namespace Web_Stadium.Services
{
    /// <summary>
    /// Sinh lịch thi đấu theo thuật toán Berger (Round Robin)
    /// Tách hoàn toàn khỏi Controller và TournamentService
    /// </summary>
    public class ScheduleService
    {
        // ══════════════════════════════════════════════════════════
        // Sinh lịch vòng tròn toàn giải (tất cả các bảng)
        // ══════════════════════════════════════════════════════════
        public List<TranDau> SinhLichVongTron(GiaiDau giaiDau)
        {
            var tatCaTran = new List<TranDau>();
            var ngayDau = giaiDau.NgayBatDau;

            foreach (var bang in giaiDau.BangDaus)
            {
                var doiTrongBang = giaiDau.DoiBongs
                    .Where(d => d.BangId == bang.Id && d.DaThanhToan)
                    .ToList();

                if (doiTrongBang.Count < 2) continue;

                var lichBang = SinhLichBerger(doiTrongBang, bang.Id, giaiDau.Id, ngayDau);
                tatCaTran.AddRange(lichBang);
            }

            return tatCaTran;
        }

        // ══════════════════════════════════════════════════════════
        // Berger Algorithm — sinh lịch 1 bảng
        // Cố định phần tử đầu, xoay vòng các phần tử còn lại
        // ══════════════════════════════════════════════════════════
        private List<TranDau> SinhLichBerger(
            List<DoiBong> dsDoi, int bangId, int giaiDauId, DateTime ngayDau)
        {
            var result = new List<TranDau>();

            // Nếu số đội lẻ → thêm BYE (null)
            var ds = new List<DoiBong?>(dsDoi.Cast<DoiBong?>());
            if (ds.Count % 2 != 0) ds.Add(null);

            int n = ds.Count;
            int soVong = n - 1;
            int soTranMoiVong = n / 2;

            for (int vong = 0; vong < soVong; vong++)
            {
                var ngayVong = ngayDau.AddDays(vong * 7); // mỗi vòng cách 1 tuần

                for (int slot = 0; slot < soTranMoiVong; slot++)
                {
                    var doiNha = ds[slot];
                    var doiKhach = ds[n - 1 - slot];

                    // Bỏ qua nếu 1 trong 2 là BYE
                    if (doiNha == null || doiKhach == null) continue;

                    // Đảo sân nhà/khách theo vòng lẻ để cân bằng
                    bool doiNhaThuat = (vong + slot) % 2 == 0;
                    result.Add(new TranDau
                    {
                        GiaiDauId = giaiDauId,
                        BangId = bangId,
                        DoiNhaId = doiNhaThuat ? doiNha.Id : doiKhach.Id,
                        DoiKhachId = doiNhaThuat ? doiKhach.Id : doiNha.Id,
                        VongDau = vong + 1,
                        LoaiVong = "VongBang",
                        NgayThiDau = ngayVong,
                        TrangThai = "Scheduled"
                    });
                }

                // Xoay Berger: cố định ds[0], xoay ds[1..n-1]
                var last = ds[n - 1];
                for (int i = n - 1; i > 1; i--)
                    ds[i] = ds[i - 1];
                ds[1] = last;
            }

            return result;
        }
    }
}