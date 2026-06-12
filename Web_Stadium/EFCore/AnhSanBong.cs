using System;
using System.Collections.Generic;

namespace Web_Stadium.EFCore;

public partial class AnhSanBong
{
    public int Id { get; set; }

    public int SanBongId { get; set; }

    public string DuongDan { get; set; } = null!;

    public string LoaiAnh { get; set; } = null!;

    public int ThuTu { get; set; }

    public string? MoTa { get; set; }

    public DateTime NgayThem { get; set; }

    public bool IsActive { get; set; }

    public virtual SanBong SanBong { get; set; } = null!;
}
