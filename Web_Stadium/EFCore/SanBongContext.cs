using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Web_Stadium.EFCore;

public partial class SanBongContext : DbContext
{
    public SanBongContext()
    {
    }

    public SanBongContext(DbContextOptions<SanBongContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<DanhGia> DanhGias { get; set; }

    public virtual DbSet<DanhMucDichVu> DanhMucDichVus { get; set; }

    public virtual DbSet<DanhMucLoaiCo> DanhMucLoaiCos { get; set; }

    public virtual DbSet<DanhMucLoaiSan> DanhMucLoaiSans { get; set; }

    public virtual DbSet<DanhMucQuan> DanhMucQuans { get; set; }

    public virtual DbSet<DatSan> DatSans { get; set; }

    public virtual DbSet<DatSanDichVu> DatSanDichVus { get; set; }

    public virtual DbSet<DichVu> DichVus { get; set; }

    public virtual DbSet<KhieuNai> KhieuNais { get; set; }

    public virtual DbSet<KhungGio> KhungGios { get; set; }

    public virtual DbSet<Matchmaking> Matchmakings { get; set; }

    public virtual DbSet<SanBong> SanBongs { get; set; }

    public virtual DbSet<StaffSanPhanCong> StaffSanPhanCongs { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<VungKhuVuc> VungKhuVucs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=TunKittt;Database=SanBongBTL;User Id=sa;Password=422005;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AuditLog__3214EC07B98758BC");

            entity.HasIndex(e => e.HanhDong, "IX_AuditLogs_HanhDong");

            entity.HasIndex(e => e.ThoiGian, "IX_AuditLogs_ThoiGian");

            entity.HasIndex(e => e.UserId, "IX_AuditLogs_UserId");

            entity.Property(e => e.DoiTuong).HasMaxLength(50);
            entity.Property(e => e.HanhDong).HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.ThoiGian)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.VaiTro).HasMaxLength(20);

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AuditLogs_User");
        });

        modelBuilder.Entity<DanhGia>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DanhGias__3214EC07D929FE0E");

            entity.HasIndex(e => new { e.UserId, e.DatSanId }, "UQ_DanhGia_User_DatSan").IsUnique();

            entity.Property(e => e.NgayDanhGia)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NhanXet).HasMaxLength(1000);
            entity.Property(e => e.SoSao).HasDefaultValue(5);

            entity.HasOne(d => d.DatSan).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.DatSanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DanhGias_DatSan");

            entity.HasOne(d => d.SanBong).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.SanBongId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DanhGias_SanBong");

            entity.HasOne(d => d.User).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DanhGias_User");
        });

        modelBuilder.Entity<DanhMucDichVu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DanhMucD__3214EC074DA53941");

            entity.ToTable("DanhMucDichVu");

            entity.Property(e => e.Icon).HasMaxLength(10);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.TenDichVu).HasMaxLength(100);
        });

        modelBuilder.Entity<DanhMucLoaiCo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DanhMucL__3214EC078B9DADB2");

            entity.ToTable("DanhMucLoaiCo");

            entity.HasIndex(e => e.MaLoai, "UQ__DanhMucL__730A5758B292C788").IsUnique();

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MaLoai).HasMaxLength(20);
            entity.Property(e => e.TenLoai).HasMaxLength(100);
        });

        modelBuilder.Entity<DanhMucLoaiSan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DanhMucL__3214EC0729420409");

            entity.ToTable("DanhMucLoaiSan");

            entity.HasIndex(e => e.MaLoai, "UQ__DanhMucL__730A5758D447AFF2").IsUnique();

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MaLoai).HasMaxLength(5);
            entity.Property(e => e.TenLoai).HasMaxLength(50);
        });

        modelBuilder.Entity<DanhMucQuan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DanhMucQ__3214EC0729878EAD");

            entity.ToTable("DanhMucQuan");

            entity.HasIndex(e => e.TenQuan, "UQ__DanhMucQ__73528DBBAC1BD689").IsUnique();

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.TenQuan).HasMaxLength(100);
            entity.Property(e => e.ThanhPho)
                .HasMaxLength(100)
                .HasDefaultValue("Hà Nội");

            entity.HasOne(d => d.VungKhuVuc).WithMany(p => p.DanhMucQuans)
                .HasForeignKey(d => d.VungKhuVucId)
                .HasConstraintName("FK_DanhMucQuan_Vung");
        });

        modelBuilder.Entity<DatSan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DatSans__3214EC07965DF6D4");

            entity.HasIndex(e => e.NgayThiDau, "IX_DatSans_NgayThiDau");

            entity.HasIndex(e => e.StaffCheckInId, "IX_DatSans_StaffCheckIn");

            entity.HasIndex(e => e.TrangThai, "IX_DatSans_TrangThai");

            entity.HasIndex(e => e.UserId, "IX_DatSans_UserId");

            entity.HasIndex(e => e.MaXacNhan, "UQ__DatSans__02DF438457E964F2").IsUnique();

            entity.Property(e => e.GhiChuSuCo).HasMaxLength(500);
            entity.Property(e => e.LoaiSuCo).HasMaxLength(20);
            entity.Property(e => e.MaXacNhan).HasMaxLength(50);
            entity.Property(e => e.NgayThiDau).HasColumnType("datetime");
            entity.Property(e => e.ThoiGianTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TienCoc).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TongTien).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("ChoDuyet");

            entity.HasOne(d => d.KhungGio).WithMany(p => p.DatSans)
                .HasForeignKey(d => d.KhungGioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DatSans_KhungGio");

            entity.HasOne(d => d.StaffCheckIn).WithMany(p => p.DatSanStaffCheckIns)
                .HasForeignKey(d => d.StaffCheckInId)
                .HasConstraintName("FK_DatSans_StaffCheckIn");

            entity.HasOne(d => d.StaffCheckOut).WithMany(p => p.DatSanStaffCheckOuts)
                .HasForeignKey(d => d.StaffCheckOutId)
                .HasConstraintName("FK_DatSans_StaffCheckOut");

            entity.HasOne(d => d.User).WithMany(p => p.DatSanUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DatSans_User");
        });

        modelBuilder.Entity<DatSanDichVu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DatSan_D__3214EC07CF8773E8");

            entity.ToTable("DatSan_DichVus");

            entity.Property(e => e.SoLuong).HasDefaultValue(1);

            entity.HasOne(d => d.DatSan).WithMany(p => p.DatSanDichVus)
                .HasForeignKey(d => d.DatSanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DatSanDichVu_DatSan");

            entity.HasOne(d => d.DichVu).WithMany(p => p.DatSanDichVus)
                .HasForeignKey(d => d.DichVuId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DatSanDichVu_DichVu");
        });

        modelBuilder.Entity<DichVu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DichVus__3214EC0794085133");

            entity.HasIndex(e => e.SanBongId, "IX_DichVus_SanBongId");

            entity.Property(e => e.Gia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.TenDichVu).HasMaxLength(100);

            entity.HasOne(d => d.DanhMucDichVu).WithMany(p => p.DichVus)
                .HasForeignKey(d => d.DanhMucDichVuId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DichVus_DanhMuc");

            entity.HasOne(d => d.SanBong).WithMany(p => p.DichVus)
                .HasForeignKey(d => d.SanBongId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DichVus_SanBong");
        });

        modelBuilder.Entity<KhieuNai>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__KhieuNai__3214EC07331207BC");

            entity.HasIndex(e => e.TrangThai, "IX_KhieuNais_TrangThai");

            entity.Property(e => e.GhiChuAdmin).HasMaxLength(500);
            entity.Property(e => e.LyDo).HasMaxLength(1000);
            entity.Property(e => e.NgayGui)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NgayXuLy).HasColumnType("datetime");
            entity.Property(e => e.SoTienHoan).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("ChoXuLy");

            entity.HasOne(d => d.AdminXuLy).WithMany(p => p.KhieuNaiAdminXuLies)
                .HasForeignKey(d => d.AdminXuLyId)
                .HasConstraintName("FK_KhieuNais_Admin");

            entity.HasOne(d => d.DatSan).WithMany(p => p.KhieuNais)
                .HasForeignKey(d => d.DatSanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KhieuNais_DatSan");

            entity.HasOne(d => d.User).WithMany(p => p.KhieuNaiUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KhieuNais_User");
        });

        modelBuilder.Entity<KhungGio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__KhungGio__3214EC07BBE9DFC3");

            entity.HasIndex(e => e.LoaiNgay, "IX_KhungGios_LoaiNgay");

            entity.HasIndex(e => e.SanBongId, "IX_KhungGios_SanBongId");

            entity.HasIndex(e => e.TrangThai, "IX_KhungGios_TrangThai");

            entity.Property(e => e.Gia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.GiaCuoiTuan).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.GiaGioVang).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.LoaiNgay)
                .HasMaxLength(20)
                .HasDefaultValue("TatCa");
            entity.Property(e => e.ThoiGianHetGiuCho).HasColumnType("datetime");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("Trong");

            entity.HasOne(d => d.SanBong).WithMany(p => p.KhungGios)
                .HasForeignKey(d => d.SanBongId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KhungGios_SanBong");
        });

        modelBuilder.Entity<Matchmaking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Matchmak__3214EC07934708EB");

            entity.HasIndex(e => e.TrangThai, "IX_Matchmakings_TrangThai");

            entity.HasIndex(e => e.DatSanId, "UQ__Matchmak__AE3C65EBA19D6212").IsUnique();

            entity.Property(e => e.MoTa).HasMaxLength(1000);
            entity.Property(e => e.NgayDang)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SoNguoiCanThem).HasDefaultValue(1);
            entity.Property(e => e.TieuDe).HasMaxLength(200);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("DangTim");

            entity.HasOne(d => d.DatSan).WithOne(p => p.Matchmaking)
                .HasForeignKey<Matchmaking>(d => d.DatSanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Matchmakings_DatSan");

            entity.HasOne(d => d.User).WithMany(p => p.Matchmakings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Matchmakings_User");
        });

        modelBuilder.Entity<SanBong>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SanBongs__3214EC07B553D465");

            entity.HasIndex(e => e.IsHidden, "IX_SanBongs_IsHidden");

            entity.HasIndex(e => e.OwnerId, "IX_SanBongs_OwnerId");

            entity.HasIndex(e => e.Quan, "IX_SanBongs_Quan");

            entity.HasIndex(e => e.TrangThaiDuyet, "IX_SanBongs_TrangThai");

            entity.Property(e => e.DiaChi).HasMaxLength(300);
            entity.Property(e => e.HinhAnh).HasMaxLength(500);
            entity.Property(e => e.LoaiCo).HasMaxLength(50);
            entity.Property(e => e.LoaiSan).HasMaxLength(5);
            entity.Property(e => e.MoTa).HasDefaultValue("");
            entity.Property(e => e.NgayKyHopDong).HasColumnType("datetime");
            entity.Property(e => e.Quan).HasMaxLength(100);
            entity.Property(e => e.TenSan).HasMaxLength(200);
            entity.Property(e => e.ThanhPho).HasMaxLength(100);
            entity.Property(e => e.TrangThaiDuyet)
                .HasMaxLength(20)
                .HasDefaultValue("ChoDuyet");
            entity.Property(e => e.TyLeCoc)
                .HasDefaultValue(0.30m)
                .HasColumnType("decimal(3, 2)");

            entity.HasOne(d => d.Owner).WithMany(p => p.SanBongs)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SanBongs_Owner");
        });

        modelBuilder.Entity<StaffSanPhanCong>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StaffSan__3214EC07A46AE590");

            entity.ToTable("StaffSanPhanCong");

            entity.HasIndex(e => e.SanBongId, "IX_StaffSan_SanBongId");

            entity.HasIndex(e => e.StaffId, "IX_StaffSan_StaffId");

            entity.HasIndex(e => new { e.StaffId, e.SanBongId }, "UQ_StaffSan").IsUnique();

            entity.Property(e => e.NgayGan)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.SanBong).WithMany(p => p.StaffSanPhanCongs)
                .HasForeignKey(d => d.SanBongId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StaffSan_SanBong");

            entity.HasOne(d => d.Staff).WithMany(p => p.StaffSanPhanCongs)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StaffSan_Staff");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC0731B6ED02");

            entity.HasIndex(e => e.OwnerIdCuaStaff, "IX_Users_OwnerIdCuaStaff");

            entity.HasIndex(e => e.VaiTro, "IX_Users_VaiTro");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534A9983397").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MatKhau).HasMaxLength(255);
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SoDienThoai).HasMaxLength(20);
            entity.Property(e => e.VaiTro)
                .HasMaxLength(20)
                .HasDefaultValue("User");

            entity.HasOne(d => d.OwnerIdCuaStaffNavigation).WithMany(p => p.InverseOwnerIdCuaStaffNavigation)
                .HasForeignKey(d => d.OwnerIdCuaStaff)
                .HasConstraintName("FK_Users_OwnerCuaStaff");
        });

        modelBuilder.Entity<VungKhuVuc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__VungKhuV__3214EC07658D9681");

            entity.HasIndex(e => e.TenVung, "UQ__VungKhuV__D64F707F63384C9C").IsUnique();

            entity.Property(e => e.DefaultZoom).HasDefaultValue(12);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Lat).HasDefaultValue(21.028500000000001);
            entity.Property(e => e.Lng).HasDefaultValue(105.85420000000001);
            entity.Property(e => e.MauSac)
                .HasMaxLength(10)
                .HasDefaultValue("#1ed760");
            entity.Property(e => e.MoTa).HasMaxLength(300);
            entity.Property(e => e.TenVung).HasMaxLength(100);
            entity.Property(e => e.TyLeHoaHong)
                .HasDefaultValue(0.10m)
                .HasColumnType("decimal(3, 2)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
