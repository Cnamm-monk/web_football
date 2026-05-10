using System;
using System.Collections.Generic;

namespace Web_Stadium.EFCore;

public partial class KhungGio
{
    public int Id { get; set; }

    public int SanBongId { get; set; }

    public TimeOnly GioBatDau { get; set; }

    public TimeOnly GioKetThuc { get; set; }

    public decimal Gia { get; set; }

    public decimal GiaGioVang { get; set; }

    public decimal GiaCuoiTuan { get; set; }

    public string LoaiNgay { get; set; } = null!;

    public string TrangThai { get; set; } = null!;

    public DateTime? ThoiGianHetGiuCho { get; set; }

    public virtual ICollection<DatSan> DatSans { get; set; } = new List<DatSan>();

    public virtual SanBong SanBong { get; set; } = null!;
}
