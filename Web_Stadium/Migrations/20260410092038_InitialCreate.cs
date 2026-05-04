using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web_Stadium.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DichVus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDichVu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DichVus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VaiTro = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SanBongs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenSan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThanhPho = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoaiSan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoaiCo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DanhGiaTrungBinh = table.Column<double>(type: "float", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    TrangThaiDuyet = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OwnerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanBongs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SanBongs_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DanhGias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SanBongId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SoSao = table.Column<int>(type: "int", nullable: false),
                    NhanXet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayDanhGia = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhGias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DanhGias_SanBongs_SanBongId",
                        column: x => x.SanBongId,
                        principalTable: "SanBongs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DanhGias_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "KhungGios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SanBongId = table.Column<int>(type: "int", nullable: false),
                    GioBatDau = table.Column<TimeSpan>(type: "time", nullable: false),
                    GioKetThuc = table.Column<TimeSpan>(type: "time", nullable: false),
                    Gia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GiaGioVang = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThoiGianHetGiuCho = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhungGios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KhungGios_SanBongs_SanBongId",
                        column: x => x.SanBongId,
                        principalTable: "SanBongs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DatSans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    KhungGioId = table.Column<int>(type: "int", nullable: false),
                    NgayThiDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TienCoc = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaXacNhan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThoiGianTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatSans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DatSans_KhungGios_KhungGioId",
                        column: x => x.KhungGioId,
                        principalTable: "KhungGios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DatSans_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DatSan_DichVus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatSanId = table.Column<int>(type: "int", nullable: false),
                    DichVuId = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatSan_DichVus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DatSan_DichVus_DatSans_DatSanId",
                        column: x => x.DatSanId,
                        principalTable: "DatSans",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DatSan_DichVus_DichVus_DichVuId",
                        column: x => x.DichVuId,
                        principalTable: "DichVus",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Matchmakings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatSanId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoNguoiCanThem = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayDang = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matchmakings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Matchmakings_DatSans_DatSanId",
                        column: x => x.DatSanId,
                        principalTable: "DatSans",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Matchmakings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DanhGias_SanBongId",
                table: "DanhGias",
                column: "SanBongId");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGias_UserId",
                table: "DanhGias",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DatSan_DichVus_DatSanId",
                table: "DatSan_DichVus",
                column: "DatSanId");

            migrationBuilder.CreateIndex(
                name: "IX_DatSan_DichVus_DichVuId",
                table: "DatSan_DichVus",
                column: "DichVuId");

            migrationBuilder.CreateIndex(
                name: "IX_DatSans_KhungGioId",
                table: "DatSans",
                column: "KhungGioId");

            migrationBuilder.CreateIndex(
                name: "IX_DatSans_UserId",
                table: "DatSans",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_KhungGios_SanBongId",
                table: "KhungGios",
                column: "SanBongId");

            migrationBuilder.CreateIndex(
                name: "IX_Matchmakings_DatSanId",
                table: "Matchmakings",
                column: "DatSanId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Matchmakings_UserId",
                table: "Matchmakings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SanBongs_OwnerId",
                table: "SanBongs",
                column: "OwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DanhGias");

            migrationBuilder.DropTable(
                name: "DatSan_DichVus");

            migrationBuilder.DropTable(
                name: "Matchmakings");

            migrationBuilder.DropTable(
                name: "DichVus");

            migrationBuilder.DropTable(
                name: "DatSans");

            migrationBuilder.DropTable(
                name: "KhungGios");

            migrationBuilder.DropTable(
                name: "SanBongs");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
