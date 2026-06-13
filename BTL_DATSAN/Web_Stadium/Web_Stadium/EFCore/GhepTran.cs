namespace Web_Stadium.EFCore;

public class GhepTran
{
    public int Id { get; set; }
    public int DatSanAId { get; set; }
    public int? DatSanBId { get; set; }
    public int UserAId { get; set; }
    public int? UserBId { get; set; }
    public string LoiNhan { get; set; } = null!;
    public int? SanChonId { get; set; }
    public int? KhungGioChonId { get; set; }
    public string? HinhThuc { get; set; }       // SanA | SanB | SanMoi
    public string TrangThai { get; set; } = "ChoXacNhan";
    // ChoXacNhan | DaXacNhan | HoanTat | TuChoi | DaHuy
    public string? GhiChu { get; set; }
    public DateTime ThoiGianTao { get; set; } = DateTime.Now;
    public DateTime? ThoiGianXuLy { get; set; }

    public virtual DatSan DatSanA { get; set; } = null!;
    public virtual DatSan? DatSanB { get; set; }
    public virtual User UserA { get; set; } = null!;
    public virtual User? UserB { get; set; }
    public virtual SanBong? SanChon { get; set; }
    public virtual KhungGio? KhungGioChon { get; set; }
}
