using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web_Stadium.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchmakingLyDoHuy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DanhGias_SanBongs_SanBongId",
                table: "DanhGias");

            migrationBuilder.DropForeignKey(
                name: "FK_DanhGias_Users_UserId",
                table: "DanhGias");

            migrationBuilder.DropForeignKey(
                name: "FK_DatSan_DichVus_DatSans_DatSanId",
                table: "DatSan_DichVus");

            migrationBuilder.DropForeignKey(
                name: "FK_DatSan_DichVus_DichVus_DichVuId",
                table: "DatSan_DichVus");

            migrationBuilder.DropForeignKey(
                name: "FK_DatSans_KhungGios_KhungGioId",
                table: "DatSans");

            migrationBuilder.DropForeignKey(
                name: "FK_DatSans_Users_UserId",
                table: "DatSans");

            migrationBuilder.DropForeignKey(
                name: "FK_KhungGios_SanBongs_SanBongId",
                table: "KhungGios");

            migrationBuilder.DropForeignKey(
                name: "FK_Matchmakings_DatSans_DatSanId",
                table: "Matchmakings");

            migrationBuilder.DropForeignKey(
                name: "FK_Matchmakings_Users_UserId",
                table: "Matchmakings");

            migrationBuilder.DropForeignKey(
                name: "FK_SanBongs_Users_OwnerId",
                table: "SanBongs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SanBongs",
                table: "SanBongs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Matchmakings",
                table: "Matchmakings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KhungGios",
                table: "KhungGios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DichVus",
                table: "DichVus");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DatSans",
                table: "DatSans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DatSan_DichVus",
                table: "DatSan_DichVus");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DanhGias",
                table: "DanhGias");

            migrationBuilder.DropIndex(
                name: "IX_DanhGias_UserId",
                table: "DanhGias");

            migrationBuilder.RenameIndex(
                name: "IX_Matchmakings_DatSanId",
                table: "Matchmakings",
                newName: "UQ__Matchmak__AE3C65EBA19D6212");

            migrationBuilder.AlterColumn<string>(
                name: "VaiTro",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "User",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "SoDienThoai",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "NgayTao",
                table: "Users",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "MatKhau",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "HoTen",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "OwnerIdCuaStaff",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TrangThaiDuyet",
                table: "SanBongs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "ChoDuyet",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ThanhPho",
                table: "SanBongs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "TenSan",
                table: "SanBongs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Quan",
                table: "SanBongs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "MoTa",
                table: "SanBongs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "LoaiSan",
                table: "SanBongs",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "LoaiCo",
                table: "SanBongs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "HinhAnh",
                table: "SanBongs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DiaChi",
                table: "SanBongs",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "DaKyHopDong",
                table: "SanBongs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "SanBongs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayKyHopDong",
                table: "SanBongs",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoiDungHopDong",
                table: "SanBongs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TyLeCoc",
                table: "SanBongs",
                type: "decimal(3,2)",
                nullable: false,
                defaultValue: 0.30m);

            migrationBuilder.AlterColumn<string>(
                name: "TrangThai",
                table: "Matchmakings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "DangTim",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "TieuDe",
                table: "Matchmakings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "SoNguoiCanThem",
                table: "Matchmakings",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "NgayDang",
                table: "Matchmakings",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "MoTa",
                table: "Matchmakings",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LyDoHuy",
                table: "Matchmakings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TrangThai",
                table: "KhungGios",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Trong",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ThoiGianHetGiuCho",
                table: "KhungGios",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GiaCuoiTuan",
                table: "KhungGios",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "LoaiNgay",
                table: "KhungGios",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "TatCa");

            migrationBuilder.AlterColumn<string>(
                name: "TenDichVu",
                table: "DichVus",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "MoTa",
                table: "DichVus",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DanhMucDichVuId",
                table: "DichVus",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "DichVus",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "SanBongId",
                table: "DichVus",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TonKho",
                table: "DichVus",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "TrangThai",
                table: "DatSans",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "ChoDuyet",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ThoiGianTao",
                table: "DatSans",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "NgayThiDau",
                table: "DatSans",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "MaXacNhan",
                table: "DatSans",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "GhiChuSuCo",
                table: "DatSans",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LoaiSuCo",
                table: "DatSans",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StaffCheckInId",
                table: "DatSans",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StaffCheckOutId",
                table: "DatSans",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TongTien",
                table: "DatSans",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<int>(
                name: "SoLuong",
                table: "DatSan_DichVus",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "SoSao",
                table: "DanhGias",
                type: "int",
                nullable: false,
                defaultValue: 5,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "NhanXet",
                table: "DanhGias",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "NgayDanhGia",
                table: "DanhGias",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "DatSanId",
                table: "DanhGias",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK__Users__3214EC0731B6ED02",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__SanBongs__3214EC07B553D465",
                table: "SanBongs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Matchmak__3214EC07934708EB",
                table: "Matchmakings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__KhungGio__3214EC07BBE9DFC3",
                table: "KhungGios",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__DichVus__3214EC0794085133",
                table: "DichVus",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__DatSans__3214EC07965DF6D4",
                table: "DatSans",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__DatSan_D__3214EC07CF8773E8",
                table: "DatSan_DichVus",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK__DanhGias__3214EC07D929FE0E",
                table: "DanhGias",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HanhDong = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DoiTuong = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DoiTuongId = table.Column<int>(type: "int", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ThoiGian = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__AuditLog__3214EC07B98758BC", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_User",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DanhMucDichVu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDichVu = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DanhMucD__3214EC074DA53941", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DanhMucLoaiCo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaLoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TenLoai = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DanhMucL__3214EC078B9DADB2", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DanhMucLoaiSan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaLoai = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    TenLoai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DanhMucL__3214EC0729420409", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KhieuNais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatSanId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LyDo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "ChoXuLy"),
                    GhiChuAdmin = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SoTienHoan = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NgayGui = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    NgayXuLy = table.Column<DateTime>(type: "datetime", nullable: true),
                    AdminXuLyId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__KhieuNai__3214EC07331207BC", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KhieuNais_Admin",
                        column: x => x.AdminXuLyId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_KhieuNais_DatSan",
                        column: x => x.DatSanId,
                        principalTable: "DatSans",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_KhieuNais_User",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StaffSanPhanCong",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    SanBongId = table.Column<int>(type: "int", nullable: false),
                    NgayGan = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__StaffSan__3214EC07A46AE590", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffSan_SanBong",
                        column: x => x.SanBongId,
                        principalTable: "SanBongs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StaffSan_Staff",
                        column: x => x.StaffId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VungKhuVucs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenVung = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TyLeHoaHong = table.Column<decimal>(type: "decimal(3,2)", nullable: false, defaultValue: 0.10m),
                    MauSac = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "#1ed760"),
                    Lat = table.Column<double>(type: "float", nullable: false, defaultValue: 21.028500000000001),
                    Lng = table.Column<double>(type: "float", nullable: false, defaultValue: 105.85420000000001),
                    DefaultZoom = table.Column<int>(type: "int", nullable: false, defaultValue: 12),
                    ThuTu = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__VungKhuV__3214EC07658D9681", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DanhMucQuan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenQuan = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ThanhPho = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "Hà Nội"),
                    ThuTu = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    VungKhuVucId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DanhMucQ__3214EC0729878EAD", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DanhMucQuan_Vung",
                        column: x => x.VungKhuVucId,
                        principalTable: "VungKhuVucs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_OwnerIdCuaStaff",
                table: "Users",
                column: "OwnerIdCuaStaff");

            migrationBuilder.CreateIndex(
                name: "IX_Users_VaiTro",
                table: "Users",
                column: "VaiTro");

            migrationBuilder.CreateIndex(
                name: "UQ__Users__A9D10534A9983397",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SanBongs_IsHidden",
                table: "SanBongs",
                column: "IsHidden");

            migrationBuilder.CreateIndex(
                name: "IX_SanBongs_Quan",
                table: "SanBongs",
                column: "Quan");

            migrationBuilder.CreateIndex(
                name: "IX_SanBongs_TrangThai",
                table: "SanBongs",
                column: "TrangThaiDuyet");

            migrationBuilder.CreateIndex(
                name: "IX_Matchmakings_TrangThai",
                table: "Matchmakings",
                column: "TrangThai");

            migrationBuilder.CreateIndex(
                name: "IX_KhungGios_LoaiNgay",
                table: "KhungGios",
                column: "LoaiNgay");

            migrationBuilder.CreateIndex(
                name: "IX_KhungGios_TrangThai",
                table: "KhungGios",
                column: "TrangThai");

            migrationBuilder.CreateIndex(
                name: "IX_DichVus_DanhMucDichVuId",
                table: "DichVus",
                column: "DanhMucDichVuId");

            migrationBuilder.CreateIndex(
                name: "IX_DichVus_SanBongId",
                table: "DichVus",
                column: "SanBongId");

            migrationBuilder.CreateIndex(
                name: "IX_DatSans_NgayThiDau",
                table: "DatSans",
                column: "NgayThiDau");

            migrationBuilder.CreateIndex(
                name: "IX_DatSans_StaffCheckIn",
                table: "DatSans",
                column: "StaffCheckInId");

            migrationBuilder.CreateIndex(
                name: "IX_DatSans_StaffCheckOutId",
                table: "DatSans",
                column: "StaffCheckOutId");

            migrationBuilder.CreateIndex(
                name: "IX_DatSans_TrangThai",
                table: "DatSans",
                column: "TrangThai");

            migrationBuilder.CreateIndex(
                name: "UQ__DatSans__02DF438457E964F2",
                table: "DatSans",
                column: "MaXacNhan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DanhGias_DatSanId",
                table: "DanhGias",
                column: "DatSanId");

            migrationBuilder.CreateIndex(
                name: "UQ_DanhGia_User_DatSan",
                table: "DanhGias",
                columns: new[] { "UserId", "DatSanId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_HanhDong",
                table: "AuditLogs",
                column: "HanhDong");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ThoiGian",
                table: "AuditLogs",
                column: "ThoiGian");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ__DanhMucL__730A5758B292C788",
                table: "DanhMucLoaiCo",
                column: "MaLoai",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__DanhMucL__730A5758D447AFF2",
                table: "DanhMucLoaiSan",
                column: "MaLoai",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DanhMucQuan_VungKhuVucId",
                table: "DanhMucQuan",
                column: "VungKhuVucId");

            migrationBuilder.CreateIndex(
                name: "UQ__DanhMucQ__73528DBBAC1BD689",
                table: "DanhMucQuan",
                column: "TenQuan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KhieuNais_AdminXuLyId",
                table: "KhieuNais",
                column: "AdminXuLyId");

            migrationBuilder.CreateIndex(
                name: "IX_KhieuNais_DatSanId",
                table: "KhieuNais",
                column: "DatSanId");

            migrationBuilder.CreateIndex(
                name: "IX_KhieuNais_TrangThai",
                table: "KhieuNais",
                column: "TrangThai");

            migrationBuilder.CreateIndex(
                name: "IX_KhieuNais_UserId",
                table: "KhieuNais",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffSan_SanBongId",
                table: "StaffSanPhanCong",
                column: "SanBongId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffSan_StaffId",
                table: "StaffSanPhanCong",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "UQ_StaffSan",
                table: "StaffSanPhanCong",
                columns: new[] { "StaffId", "SanBongId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__VungKhuV__D64F707F63384C9C",
                table: "VungKhuVucs",
                column: "TenVung",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DanhGias_DatSan",
                table: "DanhGias",
                column: "DatSanId",
                principalTable: "DatSans",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DanhGias_SanBong",
                table: "DanhGias",
                column: "SanBongId",
                principalTable: "SanBongs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DanhGias_User",
                table: "DanhGias",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DatSanDichVu_DatSan",
                table: "DatSan_DichVus",
                column: "DatSanId",
                principalTable: "DatSans",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DatSanDichVu_DichVu",
                table: "DatSan_DichVus",
                column: "DichVuId",
                principalTable: "DichVus",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DatSans_KhungGio",
                table: "DatSans",
                column: "KhungGioId",
                principalTable: "KhungGios",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DatSans_StaffCheckIn",
                table: "DatSans",
                column: "StaffCheckInId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DatSans_StaffCheckOut",
                table: "DatSans",
                column: "StaffCheckOutId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DatSans_User",
                table: "DatSans",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DichVus_DanhMuc",
                table: "DichVus",
                column: "DanhMucDichVuId",
                principalTable: "DanhMucDichVu",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DichVus_SanBong",
                table: "DichVus",
                column: "SanBongId",
                principalTable: "SanBongs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_KhungGios_SanBong",
                table: "KhungGios",
                column: "SanBongId",
                principalTable: "SanBongs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Matchmakings_DatSan",
                table: "Matchmakings",
                column: "DatSanId",
                principalTable: "DatSans",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Matchmakings_User",
                table: "Matchmakings",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SanBongs_Owner",
                table: "SanBongs",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_OwnerCuaStaff",
                table: "Users",
                column: "OwnerIdCuaStaff",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DanhGias_DatSan",
                table: "DanhGias");

            migrationBuilder.DropForeignKey(
                name: "FK_DanhGias_SanBong",
                table: "DanhGias");

            migrationBuilder.DropForeignKey(
                name: "FK_DanhGias_User",
                table: "DanhGias");

            migrationBuilder.DropForeignKey(
                name: "FK_DatSanDichVu_DatSan",
                table: "DatSan_DichVus");

            migrationBuilder.DropForeignKey(
                name: "FK_DatSanDichVu_DichVu",
                table: "DatSan_DichVus");

            migrationBuilder.DropForeignKey(
                name: "FK_DatSans_KhungGio",
                table: "DatSans");

            migrationBuilder.DropForeignKey(
                name: "FK_DatSans_StaffCheckIn",
                table: "DatSans");

            migrationBuilder.DropForeignKey(
                name: "FK_DatSans_StaffCheckOut",
                table: "DatSans");

            migrationBuilder.DropForeignKey(
                name: "FK_DatSans_User",
                table: "DatSans");

            migrationBuilder.DropForeignKey(
                name: "FK_DichVus_DanhMuc",
                table: "DichVus");

            migrationBuilder.DropForeignKey(
                name: "FK_DichVus_SanBong",
                table: "DichVus");

            migrationBuilder.DropForeignKey(
                name: "FK_KhungGios_SanBong",
                table: "KhungGios");

            migrationBuilder.DropForeignKey(
                name: "FK_Matchmakings_DatSan",
                table: "Matchmakings");

            migrationBuilder.DropForeignKey(
                name: "FK_Matchmakings_User",
                table: "Matchmakings");

            migrationBuilder.DropForeignKey(
                name: "FK_SanBongs_Owner",
                table: "SanBongs");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_OwnerCuaStaff",
                table: "Users");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "DanhMucDichVu");

            migrationBuilder.DropTable(
                name: "DanhMucLoaiCo");

            migrationBuilder.DropTable(
                name: "DanhMucLoaiSan");

            migrationBuilder.DropTable(
                name: "DanhMucQuan");

            migrationBuilder.DropTable(
                name: "KhieuNais");

            migrationBuilder.DropTable(
                name: "StaffSanPhanCong");

            migrationBuilder.DropTable(
                name: "VungKhuVucs");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Users__3214EC0731B6ED02",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_OwnerIdCuaStaff",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_VaiTro",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "UQ__Users__A9D10534A9983397",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK__SanBongs__3214EC07B553D465",
                table: "SanBongs");

            migrationBuilder.DropIndex(
                name: "IX_SanBongs_IsHidden",
                table: "SanBongs");

            migrationBuilder.DropIndex(
                name: "IX_SanBongs_Quan",
                table: "SanBongs");

            migrationBuilder.DropIndex(
                name: "IX_SanBongs_TrangThai",
                table: "SanBongs");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Matchmak__3214EC07934708EB",
                table: "Matchmakings");

            migrationBuilder.DropIndex(
                name: "IX_Matchmakings_TrangThai",
                table: "Matchmakings");

            migrationBuilder.DropPrimaryKey(
                name: "PK__KhungGio__3214EC07BBE9DFC3",
                table: "KhungGios");

            migrationBuilder.DropIndex(
                name: "IX_KhungGios_LoaiNgay",
                table: "KhungGios");

            migrationBuilder.DropIndex(
                name: "IX_KhungGios_TrangThai",
                table: "KhungGios");

            migrationBuilder.DropPrimaryKey(
                name: "PK__DichVus__3214EC0794085133",
                table: "DichVus");

            migrationBuilder.DropIndex(
                name: "IX_DichVus_DanhMucDichVuId",
                table: "DichVus");

            migrationBuilder.DropIndex(
                name: "IX_DichVus_SanBongId",
                table: "DichVus");

            migrationBuilder.DropPrimaryKey(
                name: "PK__DatSans__3214EC07965DF6D4",
                table: "DatSans");

            migrationBuilder.DropIndex(
                name: "IX_DatSans_NgayThiDau",
                table: "DatSans");

            migrationBuilder.DropIndex(
                name: "IX_DatSans_StaffCheckIn",
                table: "DatSans");

            migrationBuilder.DropIndex(
                name: "IX_DatSans_StaffCheckOutId",
                table: "DatSans");

            migrationBuilder.DropIndex(
                name: "IX_DatSans_TrangThai",
                table: "DatSans");

            migrationBuilder.DropIndex(
                name: "UQ__DatSans__02DF438457E964F2",
                table: "DatSans");

            migrationBuilder.DropPrimaryKey(
                name: "PK__DatSan_D__3214EC07CF8773E8",
                table: "DatSan_DichVus");

            migrationBuilder.DropPrimaryKey(
                name: "PK__DanhGias__3214EC07D929FE0E",
                table: "DanhGias");

            migrationBuilder.DropIndex(
                name: "IX_DanhGias_DatSanId",
                table: "DanhGias");

            migrationBuilder.DropIndex(
                name: "UQ_DanhGia_User_DatSan",
                table: "DanhGias");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OwnerIdCuaStaff",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DaKyHopDong",
                table: "SanBongs");

            migrationBuilder.DropColumn(
                name: "IsHidden",
                table: "SanBongs");

            migrationBuilder.DropColumn(
                name: "NgayKyHopDong",
                table: "SanBongs");

            migrationBuilder.DropColumn(
                name: "NoiDungHopDong",
                table: "SanBongs");

            migrationBuilder.DropColumn(
                name: "TyLeCoc",
                table: "SanBongs");

            migrationBuilder.DropColumn(
                name: "LyDoHuy",
                table: "Matchmakings");

            migrationBuilder.DropColumn(
                name: "GiaCuoiTuan",
                table: "KhungGios");

            migrationBuilder.DropColumn(
                name: "LoaiNgay",
                table: "KhungGios");

            migrationBuilder.DropColumn(
                name: "DanhMucDichVuId",
                table: "DichVus");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "DichVus");

            migrationBuilder.DropColumn(
                name: "SanBongId",
                table: "DichVus");

            migrationBuilder.DropColumn(
                name: "TonKho",
                table: "DichVus");

            migrationBuilder.DropColumn(
                name: "GhiChuSuCo",
                table: "DatSans");

            migrationBuilder.DropColumn(
                name: "LoaiSuCo",
                table: "DatSans");

            migrationBuilder.DropColumn(
                name: "StaffCheckInId",
                table: "DatSans");

            migrationBuilder.DropColumn(
                name: "StaffCheckOutId",
                table: "DatSans");

            migrationBuilder.DropColumn(
                name: "TongTien",
                table: "DatSans");

            migrationBuilder.DropColumn(
                name: "DatSanId",
                table: "DanhGias");

            migrationBuilder.RenameIndex(
                name: "UQ__Matchmak__AE3C65EBA19D6212",
                table: "Matchmakings",
                newName: "IX_Matchmakings_DatSanId");

            migrationBuilder.AlterColumn<string>(
                name: "VaiTro",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "User");

            migrationBuilder.AlterColumn<string>(
                name: "SoDienThoai",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "NgayTao",
                table: "Users",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<string>(
                name: "MatKhau",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "HoTen",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "TrangThaiDuyet",
                table: "SanBongs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "ChoDuyet");

            migrationBuilder.AlterColumn<string>(
                name: "ThanhPho",
                table: "SanBongs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "TenSan",
                table: "SanBongs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Quan",
                table: "SanBongs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "MoTa",
                table: "SanBongs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "LoaiSan",
                table: "SanBongs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(5)",
                oldMaxLength: 5);

            migrationBuilder.AlterColumn<string>(
                name: "LoaiCo",
                table: "SanBongs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "HinhAnh",
                table: "SanBongs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DiaChi",
                table: "SanBongs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300);

            migrationBuilder.AlterColumn<string>(
                name: "TrangThai",
                table: "Matchmakings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "DangTim");

            migrationBuilder.AlterColumn<string>(
                name: "TieuDe",
                table: "Matchmakings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<int>(
                name: "SoNguoiCanThem",
                table: "Matchmakings",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<DateTime>(
                name: "NgayDang",
                table: "Matchmakings",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<string>(
                name: "MoTa",
                table: "Matchmakings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TrangThai",
                table: "KhungGios",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Trong");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ThoiGianHetGiuCho",
                table: "KhungGios",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenDichVu",
                table: "DichVus",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "MoTa",
                table: "DichVus",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TrangThai",
                table: "DatSans",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "ChoDuyet");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ThoiGianTao",
                table: "DatSans",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "NgayThiDau",
                table: "DatSans",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AlterColumn<string>(
                name: "MaXacNhan",
                table: "DatSans",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "SoLuong",
                table: "DatSan_DichVus",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "SoSao",
                table: "DanhGias",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 5);

            migrationBuilder.AlterColumn<string>(
                name: "NhanXet",
                table: "DanhGias",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "NgayDanhGia",
                table: "DanhGias",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SanBongs",
                table: "SanBongs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Matchmakings",
                table: "Matchmakings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KhungGios",
                table: "KhungGios",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DichVus",
                table: "DichVus",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DatSans",
                table: "DatSans",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DatSan_DichVus",
                table: "DatSan_DichVus",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DanhGias",
                table: "DanhGias",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGias_UserId",
                table: "DanhGias",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DanhGias_SanBongs_SanBongId",
                table: "DanhGias",
                column: "SanBongId",
                principalTable: "SanBongs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DanhGias_Users_UserId",
                table: "DanhGias",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DatSan_DichVus_DatSans_DatSanId",
                table: "DatSan_DichVus",
                column: "DatSanId",
                principalTable: "DatSans",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DatSan_DichVus_DichVus_DichVuId",
                table: "DatSan_DichVus",
                column: "DichVuId",
                principalTable: "DichVus",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DatSans_KhungGios_KhungGioId",
                table: "DatSans",
                column: "KhungGioId",
                principalTable: "KhungGios",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DatSans_Users_UserId",
                table: "DatSans",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_KhungGios_SanBongs_SanBongId",
                table: "KhungGios",
                column: "SanBongId",
                principalTable: "SanBongs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Matchmakings_DatSans_DatSanId",
                table: "Matchmakings",
                column: "DatSanId",
                principalTable: "DatSans",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Matchmakings_Users_UserId",
                table: "Matchmakings",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SanBongs_Users_OwnerId",
                table: "SanBongs",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
