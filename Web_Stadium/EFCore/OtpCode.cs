using System;
using System.Collections.Generic;

namespace Web_Stadium.EFCore;

public partial class OtpCode
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string SoDienThoai { get; set; } = null!;

    public string MaOtp { get; set; } = null!;

    public DateTime NgayTao { get; set; }

    public DateTime NgayHetHan { get; set; }

    public bool IsUsed { get; set; }

    public virtual User User { get; set; } = null!;
}
