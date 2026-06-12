using System;
using System.Collections.Generic;

namespace Web_Stadium.EFCore;

public partial class UserVoucher
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int VoucherId { get; set; }

    public string MaSuDung { get; set; } = null!;

    public DateTime NgayDoi { get; set; }

    public DateTime NgayHetHan { get; set; }

    public bool IsUsed { get; set; }

    public DateTime? NgaySuDung { get; set; }

    public int? DatSanId { get; set; }

    public virtual DatSan? DatSan { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual Voucher Voucher { get; set; } = null!;
}
