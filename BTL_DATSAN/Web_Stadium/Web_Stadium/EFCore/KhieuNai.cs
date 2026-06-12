using System;
using System.Collections.Generic;

namespace Web_Stadium.EFCore;

public partial class KhieuNai
{
    public int Id { get; set; }

    public int DatSanId { get; set; }

    public int UserId { get; set; }

    public string LyDo { get; set; } = null!;

    public string TrangThai { get; set; } = null!;

    public string? GhiChuAdmin { get; set; }

    public decimal? SoTienHoan { get; set; }

    public DateTime NgayGui { get; set; }

    public DateTime? NgayXuLy { get; set; }

    public int? AdminXuLyId { get; set; }

    public virtual User? AdminXuLy { get; set; }

    public virtual DatSan DatSan { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
