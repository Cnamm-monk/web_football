using System;
using System.Collections.Generic;

namespace Web_Stadium.EFCore;

public partial class AuditLog
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string VaiTro { get; set; } = null!;

    public string HanhDong { get; set; } = null!;

    public string DoiTuong { get; set; } = null!;

    public int DoiTuongId { get; set; }

    public string? MoTa { get; set; }

    public string? IpAddress { get; set; }

    public DateTime ThoiGian { get; set; }

    public virtual User User { get; set; } = null!;
}
