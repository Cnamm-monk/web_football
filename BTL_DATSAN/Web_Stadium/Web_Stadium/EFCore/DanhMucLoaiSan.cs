using System;
using System.Collections.Generic;

namespace Web_Stadium.EFCore;

public partial class DanhMucLoaiSan
{
    public int Id { get; set; }

    public string MaLoai { get; set; } = null!;

    public string TenLoai { get; set; } = null!;

    public bool IsActive { get; set; }
}
