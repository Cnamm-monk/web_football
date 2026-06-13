namespace Web_Stadium.EFCore;

public class YeuCauDoiGio
{
    public int Id { get; set; }
    public int DatSanId { get; set; }
    public int UserId { get; set; }
    public int KhungGioMoiId { get; set; }
    public DateTime NgayMoi { get; set; }
    public string LyDo { get; set; } = null!;
    public string TrangThai { get; set; } = "ChoXuLy"; // ChoXuLy | DaPheDuyet | TuChoi
    public string? GhiChuStaff { get; set; }
    public string? GhiChuOwner { get; set; }
    public string? GhiChuAdmin { get; set; }
    public int? StaffXuLyId { get; set; }
    public int? OwnerXuLyId { get; set; }
    public int? AdminXuLyId { get; set; }
    public DateTime ThoiGianTao { get; set; } = DateTime.Now;
    public DateTime? ThoiGianXuLy { get; set; }

    public virtual DatSan DatSan { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual KhungGio KhungGioMoi { get; set; } = null!;
    public virtual User? StaffXuLy { get; set; }
    public virtual User? OwnerXuLy { get; set; }
    public virtual User? AdminXuLy { get; set; }
}
