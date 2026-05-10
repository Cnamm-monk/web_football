using System;
using System.Collections.Generic;

namespace Web_Stadium.EFCore;

public partial class VungKhuVuc
{
    public int Id { get; set; }

    public string TenVung { get; set; } = null!;

    public string? MoTa { get; set; }

    public decimal TyLeHoaHong { get; set; }

    public string MauSac { get; set; } = null!;

    public double Lat { get; set; }

    public double Lng { get; set; }

    public int DefaultZoom { get; set; }

    public int ThuTu { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<DanhMucQuan> DanhMucQuans { get; set; } = new List<DanhMucQuan>();
}
