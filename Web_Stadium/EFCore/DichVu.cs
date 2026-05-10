using System;
using System.Collections.Generic;

namespace Web_Stadium.EFCore;

public partial class DichVu
{
    public int Id { get; set; }

    public int SanBongId { get; set; }

    public int DanhMucDichVuId { get; set; }

    public string TenDichVu { get; set; } = null!;

    public decimal Gia { get; set; }

    public int TonKho { get; set; }

    public bool IsActive { get; set; }

    public string? MoTa { get; set; }

    public virtual DanhMucDichVu DanhMucDichVu { get; set; } = null!;

    public virtual ICollection<DatSanDichVu> DatSanDichVus { get; set; } = new List<DatSanDichVu>();

    public virtual SanBong SanBong { get; set; } = null!;
}
