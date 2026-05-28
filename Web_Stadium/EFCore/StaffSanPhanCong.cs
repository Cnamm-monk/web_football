using System;
using System.Collections.Generic;

namespace Web_Stadium.EFCore;

public partial class StaffSanPhanCong
{
    public int Id { get; set; }

    public int StaffId { get; set; }

    public int SanBongId { get; set; }

    public DateTime NgayGan { get; set; }

    public virtual SanBong SanBong { get; set; } = null!;

    public virtual User Staff { get; set; } = null!;
}
