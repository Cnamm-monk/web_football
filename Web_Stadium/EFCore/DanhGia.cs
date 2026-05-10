using System;
using System.Collections.Generic;

namespace Web_Stadium.EFCore;

public partial class DanhGia
{
    public int Id { get; set; }

    public int SanBongId { get; set; }

    public int UserId { get; set; }

    public int DatSanId { get; set; }

    public int SoSao { get; set; }

    public string? NhanXet { get; set; }

    public DateTime NgayDanhGia { get; set; }

    public virtual DatSan DatSan { get; set; } = null!;

    public virtual SanBong SanBong { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
