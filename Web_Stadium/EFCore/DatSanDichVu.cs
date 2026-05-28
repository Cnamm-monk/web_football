using System;
using System.Collections.Generic;

namespace Web_Stadium.EFCore;

public partial class DatSanDichVu
{
    public int Id { get; set; }

    public int DatSanId { get; set; }

    public int DichVuId { get; set; }

    public int SoLuong { get; set; }

    public virtual DatSan DatSan { get; set; } = null!;

    public virtual DichVu DichVu { get; set; } = null!;
}
