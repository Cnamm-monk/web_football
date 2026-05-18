using System;
namespace Web_Stadium.EFCore;

public class AnhSanBong
{
    public int Id { get; set; }
    public int SanBongId { get; set; }
    public string DuongDan { get; set; } = null!;  // URL hoac /images/san/...
    public string LoaiAnh { get; set; } = "Upload"; // "Upload" | "URL"
    public int ThuTu { get; set; } = 0;
    public string? MoTa { get; set; }
    public DateTime NgayThem { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } = true;

    // Navigation
    public virtual SanBong SanBong { get; set; } = null!;
}