using System;
using System.Collections.Generic;

namespace Web_Stadium.EFCore;

public partial class DanhMucDichVu
{
    public int Id { get; set; }

    public string TenDichVu { get; set; } = null!;

    public string? Icon { get; set; }

    public string? MoTa { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<DichVu> DichVus { get; set; } = new List<DichVu>();
}
