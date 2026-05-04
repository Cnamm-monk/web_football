using System;
using System.Collections.Generic;

namespace Web_Stadium.End
{
    public class DanhGia
    {
        public int Id { get; set; }
        
        // Foreign key -> SanBong
        public int SanBongId { get; set; }
        
        // Foreign key -> Users
        public int UserId { get; set; }

        public int SoSao { get; set; } // 1 đến 5 sao
        public string? NhanXet { get; set; }
        public DateTime NgayDanhGia { get; set; } = DateTime.Now;
        
        // Navigation properties
        public SanBong SanBong { get; set; }
        public User User { get; set; }
    }
}
