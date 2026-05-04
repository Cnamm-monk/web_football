using System;
using System.Collections.Generic;

namespace Web_Stadium.End
{
    public class Matchmaking
    {
        public int Id { get; set; }

        // Foregin key -> DatSan (trận đấu được tạo từ đặt sân nào)
        public int DatSanId { get; set; }

        // Foreign key -> User (người tạo trận đấu)
        public int UserId { get; set; }
        
        public string TieuDe { get; set; }
        public string? MoTa { get; set; }

        // So nguoi can them
        public int SoNguoiCanThem { get; set; }
        // "DangTim" | "DaDu" | "DaHuy"
        public string TrangThai { get; set; } = "DangTim";
        public DateTime NgayDang { get; set; } = DateTime.Now;

        // Navigation properties
        public User User { get; set; }
        public DatSan DatSan { get; set; }
    }
}
