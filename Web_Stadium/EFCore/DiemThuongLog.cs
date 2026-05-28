using System;
using System.Collections.Generic;

namespace Web_Stadium.EFCore;

public partial class DiemThuongLog
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int SoDiem { get; set; }

    public int SoDuSauGd { get; set; }

    public string LoaiSuKien { get; set; } = null!;

    public string? GhiChu { get; set; }

    public int? DatSanId { get; set; }

    public DateTime ThoiGian { get; set; }

    public virtual DatSan? DatSan { get; set; }

    public virtual User User { get; set; } = null!;
}
