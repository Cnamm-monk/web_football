using System;
using System.Collections.Generic;

namespace Web_Stadium.End
{
    public class SanBong
    {
        public int Id { get; set; }
        public string TenSan { get; set; }
        public string DiaChi { get; set; }
        public string Quan { get; set; }
        public string ThanhPho { get; set; }

        // "5" | "7" | "11"
        public string LoaiSan { get; set; }

        // "Sân cỏ nhân tạo" | "Sân cỏ tự nhiên"
        public string LoaiCo { get; set; }

        public string? HinhAnh { get; set; }
        public string MoTa { get; set; }
        public double DanhGiaTrungBinh { get; set; } = 0;

        // Tọa độ bản đồ - yêu cầu tích hợp bản đồ
        public double Latitude { get; set; } // Vĩ độ
        public double Longitude { get; set; } // Kinh độ

        // Admin phê duyệt sân mới
        // "ChoDuyet" | "DaDuyet" | "TuChoi"
        public string TrangThaiDuyet { get; set; }

        // Foreign key -> Users
        public int OwnerId { get; set; }

        //Navigation properties
        public User Owner { get; set; }
        public ICollection<KhungGio> KhungGios { get; set; }
        public ICollection<DanhGia> DanhGias { get; set; }
    }
}
