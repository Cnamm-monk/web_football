using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_Stadium.End
{
    public class KhungGio
    {
        public int Id { get; set; }

        // Foregin key -> SanBong
        public int SanBongId { get; set; }

        public TimeSpan GioBatDau { get; set; }
        public TimeSpan GioKetThuc { get; set; }

        // Giá Ngày thường
        [Column(TypeName = "decimal(18,2)")]
        public decimal Gia { get; set; }

        // Giá Cuối tuần / sau 17h00
        [Column(TypeName = "decimal(18,2)")]
        public decimal GiaGioVang { get; set; }

        // "Còn trống" | "Đã đặt" | "Đang giữ"
        public string TrangThai { get; set; } = "Trong";
        
        // Thời gian out checking/ hết thời gian giữ chỗ
         public DateTime? ThoiGianHetGiuCho { get; set; }

        // Navigation properties
        public SanBong SanBong { get; set; }
        public ICollection<DatSan> DanhSachDatSan { get; set; }

    }
}
