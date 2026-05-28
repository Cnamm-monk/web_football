using System;
using System.Collections.Generic;

namespace Web_Stadium.EFCore;

public partial class SanYeuThich
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int SanBongId { get; set; }

    public DateTime NgayThem { get; set; }

    public virtual SanBong SanBong { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
