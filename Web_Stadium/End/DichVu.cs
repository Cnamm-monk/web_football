using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_Stadium.End
{
    public class DichVu
    {
        public int Id { get; set; }

        // "Nuoc uong" | "Thue bong" | "Thue trong tai" | "Thue ao"
        public string TenDichVu { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Gia { get; set; }
        public string? MoTa { get; set; }

        // Navigation properties
        public ICollection<DatSan_DichVu> DatSan_DichVus { get; set; }
    }
}
