using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Web_Stadium.EFCore;

namespace Web_Stadium.End
{
    // ── VungKhuVuc — Admin quản lý phân vùng hoa hồng ───────────
    public class VungKhuVuc
    {
        public int Id { get; set; }
        public string TenVung { get; set; }        // "Nội ô trung tâm"
        public string? MoTa { get; set; }
        [Column(TypeName = "decimal(3,2)")]
        public decimal TyLeHoaHong { get; set; } = 0.10m;  // 0.10 = 10%
        public string MauSac { get; set; } = "#1ed760";     // hex color
        public double Lat { get; set; } = 21.0285;          // toạ độ trung tâm vùng
        public double Lng { get; set; } = 105.8542;
        public int DefaultZoom { get; set; } = 12;
        public int ThuTu { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<DanhMucQuan> DanhSachQuan { get; set; }
    }

    // ── DanhMucQuan — cập nhật: thêm VungKhuVucId ───────────────
    public class DanhMucQuan
    {
        public int Id { get; set; }
        public string TenQuan { get; set; }
        public string ThanhPho { get; set; } = "Hà Nội";
        public int ThuTu { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        // FK → VungKhuVuc — Admin gán quận vào vùng
        public int? VungKhuVucId { get; set; }

        // Navigation
        public VungKhuVuc? VungKhuVuc { get; set; }
    }

    // ── DanhMucLoaiSan ───────────────────────────────────────────
    public class DanhMucLoaiSan
    {
        public int Id { get; set; }
        public string MaLoai { get; set; }
        public string TenLoai { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // ── DanhMucLoaiCo ────────────────────────────────────────────
    public class DanhMucLoaiCo
    {
        public int Id { get; set; }
        public string MaLoai { get; set; }
        public string TenLoai { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // ── DanhMucDichVu ────────────────────────────────────────────
    public class DanhMucDichVu
    {
        public int Id { get; set; }
        public string TenDichVu { get; set; }
        public string? Icon { get; set; }
        public string? MoTa { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<DichVu> DichVus { get; set; }
    }

    // ── StaffSanPhanCong ─────────────────────────────────────────
    public class StaffSanPhanCong
    {
        public int Id { get; set; }
        public int StaffId { get; set; }
        public int SanBongId { get; set; }
        public DateTime NgayGan { get; set; } = DateTime.Now;
        public User Staff { get; set; }
        public SanBong SanBong { get; set; }
    }

    // ── KhieuNai ─────────────────────────────────────────────────
    public class KhieuNai
    {
        public int Id { get; set; }
        public int DatSanId { get; set; }
        public int UserId { get; set; }
        public string LyDo { get; set; }
        public string TrangThai { get; set; } = "ChoXuLy";
        public string? GhiChuAdmin { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? SoTienHoan { get; set; }
        public DateTime NgayGui { get; set; } = DateTime.Now;
        public DateTime? NgayXuLy { get; set; }
        public int? AdminXuLyId { get; set; }
        public DatSan DatSan { get; set; }
        public User User { get; set; }
        public User? AdminXuLy { get; set; }
    }

    // ── AuditLog ─────────────────────────────────────────────────
    public class AuditLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string VaiTro { get; set; }
        public string HanhDong { get; set; }
        public string DoiTuong { get; set; }
        public int DoiTuongId { get; set; }
        public string? MoTa { get; set; }
        public string? IpAddress { get; set; }
        public DateTime ThoiGian { get; set; } = DateTime.Now;
        public User User { get; set; }
    }
}