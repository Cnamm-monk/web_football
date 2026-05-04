using System;
using System.Collections.Generic;

namespace Web_Stadium.End
{
    public class DatSan_DichVu
    {
        public int Id { get; set; }
        
        // Foreign key -> DatSan
        public int DatSanId { get; set; }
        
        // Foreign key -> DichVu
        public int DichVuId { get; set; }
        
        public int SoLuong { get; set; } = 1;
        
        // Navigation properties
        public DatSan DatSan { get; set; }
        public DichVu DichVu { get; set; }
    }
}
