using System;
using System.Collections.Generic;

namespace Web_Stadium.EFCore;

public partial class DanhMucQuan
{
    public int Id { get; set; }

    public string TenQuan { get; set; } = null!;

    public string ThanhPho { get; set; } = null!;

    public int ThuTu { get; set; }

    public bool IsActive { get; set; }

    public int? VungKhuVucId { get; set; }

    public virtual VungKhuVuc? VungKhuVuc { get; set; }
}
