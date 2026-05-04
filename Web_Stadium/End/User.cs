using System;
using System.Collections.Generic;

namespace Web_Stadium.End
{
    public class User
    {
        public int Id { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string MatKhau { get; set; }
        public string? SoDienThoai { get; set; }

        // "User" | "Owner" | "Staff" | "Admin"
        public string VaiTro { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation properties
        public ICollection<DatSan> DanhSachDatSan { get; set; }
        public ICollection<SanBong> SanBongSoHuu { get; set; }
        public ICollection<DanhGia> DanhGias { get; set; }
        public ICollection<Matchmaking> Matchmakings { get; set; }
    }
}
