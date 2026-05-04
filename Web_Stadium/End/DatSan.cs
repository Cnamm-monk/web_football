using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_Stadium.End
{
    public class DatSan
    {
        public int Id { get; set; }
        
        // Foreign key -> User
        public int UserId { get; set; }
        
        // Foreign key -> KhungGio
        public int KhungGioId { get; set; }

        public DateTime NgayThiDau { get; set; }

        // Tiền cộc (đặt cọc trước khi đến sân, có thể là 30% hoặc 50% tùy chính sách của sân)
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TienCoc { get; set; }

        // Mã xác nhân đặt sân (có thể là một chuỗi ngẫu nhiên hoặc mã QR)
        public string MaXacNhan { get; set; }

        // "Choduyet" | "DaXacNhan" | "DaHuy" | "HoanThanh"
        public string TrangThai { get; set; } = "Choduyet";
        public DateTime ThoiGianTao { get; set; } = DateTime.Now;

        //Navigation properties
        public User User { get; set; }
        public KhungGio KhungGio { get; set; }
        public ICollection<DatSan_DichVu> DichVuKemTheo { get; set; }
        public Matchmaking? matchmaking { get; set; }
    }
}
