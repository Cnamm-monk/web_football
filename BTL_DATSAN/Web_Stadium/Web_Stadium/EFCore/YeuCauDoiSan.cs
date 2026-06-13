namespace Web_Stadium.EFCore;

public class YeuCauDoiSan
{
    public int Id { get; set; }
    public int DatSanId { get; set; }
    public int UserId { get; set; }
    public int SanMoiId { get; set; }
    public int KhungGioMoiId { get; set; }
    public DateTime NgayThiDau { get; set; }
    public string LyDo { get; set; } = null!;
    public string TrangThai { get; set; } = "ChoXuLy"; // ChoXuLy | DaPheDuyet | TuChoi
    public decimal ChenhLechGia { get; set; }
    public string? GhiChuOwner { get; set; }
    public int? OwnerXuLyId { get; set; }
    public DateTime ThoiGianTao { get; set; } = DateTime.Now;
    public DateTime? ThoiGianXuLy { get; set; }

    public virtual DatSan DatSan { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual SanBong SanMoi { get; set; } = null!;
    public virtual KhungGio KhungGioMoi { get; set; } = null!;
    public virtual User? OwnerXuLy { get; set; }
}
