using System;
using System.Collections.Generic;

namespace Web_Stadium.EFCore;

public partial class Voucher
{
    public int Id { get; set; }

    public string MaVoucher { get; set; } = null!;

    public string TenVoucher { get; set; } = null!;

    public string? MoTa { get; set; }

    public string LoaiGiam { get; set; } = null!;

    public decimal GiaTriGiam { get; set; }

    public decimal? GiamToiDa { get; set; }

    public int DiemCanDoi { get; set; }

    public int SoNgayHieuLuc { get; set; }

    public bool IsActive { get; set; }

    public DateTime NgayTao { get; set; }

    public virtual ICollection<UserVoucher> UserVouchers { get; set; } = new List<UserVoucher>();
}
