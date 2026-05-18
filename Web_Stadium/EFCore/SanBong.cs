using System;
using System.Collections.Generic;

namespace Web_Stadium.EFCore;

public partial class SanBong
{
    public int Id { get; set; }

    public string TenSan { get; set; } = null!;

    public string DiaChi { get; set; } = null!;

    public string Quan { get; set; } = null!;

    public string ThanhPho { get; set; } = null!;

    public string LoaiSan { get; set; } = null!;

    public string LoaiCo { get; set; } = null!;

    public string? HinhAnh { get; set; }

    public string MoTa { get; set; } = null!;

    public double DanhGiaTrungBinh { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public string TrangThaiDuyet { get; set; } = null!;

    public decimal TyLeCoc { get; set; }

    public bool IsHidden { get; set; }

    public int OwnerId { get; set; }

    public bool DaKyHopDong { get; set; }

    public DateTime? NgayKyHopDong { get; set; }

    public string? NoiDungHopDong { get; set; }

    public virtual ICollection<DanhGia> DanhGia { get; set; } = new List<DanhGia>();

    public virtual ICollection<DichVu> DichVus { get; set; } = new List<DichVu>();

    public virtual ICollection<KhungGio> KhungGios { get; set; } = new List<KhungGio>();

    public virtual User Owner { get; set; } = null!;

    public virtual ICollection<StaffSanPhanCong> StaffSanPhanCongs { get; set; } = new List<StaffSanPhanCong>();

    // Anh san do Owner upload
    public virtual ICollection<AnhSanBong> AnhSanBongs { get; set; } = new List<AnhSanBong>();
}