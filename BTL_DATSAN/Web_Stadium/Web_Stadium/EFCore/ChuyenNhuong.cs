namespace Web_Stadium.EFCore;

public class ChuyenNhuong
{
    public int Id { get; set; }
    public int DatSanId { get; set; }
    public int UserAId { get; set; }
    public int? UserBId { get; set; }
    public string TieuDe { get; set; } = null!;
    public string? MoTa { get; set; }
    public decimal GiaChuyenNhuong { get; set; }
    public string? SoDienThoaiLienHe { get; set; }
    // DangTim | ChoXacNhan | ChoStaff | ChoOwner | HoanTat | TuChoi | DaHuy
    public string TrangThai { get; set; } = "DangTim";
    public string? GhiChuStaff { get; set; }
    public string? GhiChuOwner { get; set; }
    public int? StaffXuLyId { get; set; }
    public int? OwnerXuLyId { get; set; }
    public bool DaChuyenNhuong { get; set; } = false;
    public DateTime ThoiGianTao { get; set; } = DateTime.Now;
    public DateTime? ThoiGianXuLy { get; set; }

    public virtual DatSan DatSan { get; set; } = null!;
    public virtual User UserA { get; set; } = null!;
    public virtual User? UserB { get; set; }
    public virtual User? StaffXuLy { get; set; }
    public virtual User? OwnerXuLy { get; set; }
}
