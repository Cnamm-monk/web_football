using System;
using System.Collections.Generic;
namespace Web_Stadium.EFCore;
public partial class Matchmaking
{
    public int Id { get; set; }
    public int DatSanId { get; set; }
    public int UserId { get; set; }
    public string TieuDe { get; set; } = null!;
    public string? MoTa { get; set; }
    public int SoNguoiCanThem { get; set; }

    // "DangTim" | "TuDongHuy" | "DaCoDoiLienHe" | "DaKetThuc"
    public string TrangThai { get; set; } = "DangTim";

    // Ly do huy (chi co khi TrangThai = "TuDongHuy")
    public string? LyDoHuy { get; set; }

    public DateTime NgayDang { get; set; }
    public virtual DatSan DatSan { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}