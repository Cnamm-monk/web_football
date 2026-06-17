using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web_Stadium.Migrations
{
    /// <inheritdoc />
    public partial class AddVoucherSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DaXacThucSdt",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DiemHienTai",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LoaiVoucherApDung",
                table: "DatSans",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TienGiam",
                table: "DatSans",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TienGoc",
                table: "DatSans",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "VoucherId",
                table: "DatSans",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SoSaoCoSoVatChat",
                table: "DanhGias",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SoSaoNhanVien",
                table: "DanhGias",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AnhSanBongs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SanBongId = table.Column<int>(type: "int", nullable: false),
                    DuongDan = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    LoaiAnh = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Upload"),
                    ThuTu = table.Column<int>(type: "int", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NgayThem = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhSanBongs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnhSanBongs_SanBong",
                        column: x => x.SanBongId,
                        principalTable: "SanBongs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiemThuongLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SoDiem = table.Column<int>(type: "int", nullable: false),
                    SoDuSauGD = table.Column<int>(type: "int", nullable: false),
                    LoaiSuKien = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DatSanId = table.Column<int>(type: "int", nullable: true),
                    ThoiGian = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DiemThuo__3214EC07082AC8BB", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiemLog_DatSan",
                        column: x => x.DatSanId,
                        principalTable: "DatSans",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DiemLog_User",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OtpCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MaOtp = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    NgayHetHan = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(dateadd(minute,(5),getdate()))"),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__OtpCodes__3214EC072CAD03AF", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OtpCodes_User",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SanYeuThichs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SanBongId = table.Column<int>(type: "int", nullable: false),
                    NgayThem = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SanYeuTh__3214EC07643721CD", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SanYeuThich_SanBong",
                        column: x => x.SanBongId,
                        principalTable: "SanBongs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SanYeuThich_User",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vouchers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaVoucher = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenVoucher = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LoaiGiam = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "PhanTram"),
                    GiaTriGiam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GiamToiDa = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DiemCanDoi = table.Column<int>(type: "int", nullable: false),
                    SoNgayHieuLuc = table.Column<int>(type: "int", nullable: false, defaultValue: 30),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    NgayTao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    LoaiVoucher = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "HeThong"),
                    SanBongId = table.Column<int>(type: "int", nullable: true),
                    OwnerId = table.Column<int>(type: "int", nullable: true),
                    SoLuong = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DaDung = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    NgayBatDau = table.Column<DateTime>(type: "datetime", nullable: false),
                    NgayHetHan = table.Column<DateTime>(type: "datetime", nullable: false),
                    DieuKienToiThieu = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Vouchers__3214EC07092807B4", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vouchers_Owner",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Vouchers_SanBong",
                        column: x => x.SanBongId,
                        principalTable: "SanBongs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserVouchers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    VoucherId = table.Column<int>(type: "int", nullable: false),
                    MaSuDung = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NgayDoi = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    NgayHetHan = table.Column<DateTime>(type: "datetime", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    NgaySuDung = table.Column<DateTime>(type: "datetime", nullable: true),
                    DatSanId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserVouc__3214EC0796E42189", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserVoucher_DatSan",
                        column: x => x.DatSanId,
                        principalTable: "DatSans",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserVoucher_User",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserVoucher_Voucher",
                        column: x => x.VoucherId,
                        principalTable: "Vouchers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DatSans_VoucherId",
                table: "DatSans",
                column: "VoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_AnhSanBongs_SanBong",
                table: "AnhSanBongs",
                columns: new[] { "SanBongId", "ThuTu" });

            migrationBuilder.CreateIndex(
                name: "IX_DiemLog_ThoiGian",
                table: "DiemThuongLogs",
                column: "ThoiGian",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_DiemLog_UserId",
                table: "DiemThuongLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DiemThuongLogs_DatSanId",
                table: "DiemThuongLogs",
                column: "DatSanId");

            migrationBuilder.CreateIndex(
                name: "IX_OtpCodes_HetHan",
                table: "OtpCodes",
                column: "NgayHetHan");

            migrationBuilder.CreateIndex(
                name: "IX_OtpCodes_UserId",
                table: "OtpCodes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SanYeuThich_SanBongId",
                table: "SanYeuThichs",
                column: "SanBongId");

            migrationBuilder.CreateIndex(
                name: "IX_SanYeuThich_UserId",
                table: "SanYeuThichs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ_SanYeuThich",
                table: "SanYeuThichs",
                columns: new[] { "UserId", "SanBongId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserVoucher_HetHan",
                table: "UserVouchers",
                column: "NgayHetHan");

            migrationBuilder.CreateIndex(
                name: "IX_UserVoucher_IsUsed",
                table: "UserVouchers",
                column: "IsUsed");

            migrationBuilder.CreateIndex(
                name: "IX_UserVoucher_UserId",
                table: "UserVouchers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserVouchers_DatSanId",
                table: "UserVouchers",
                column: "DatSanId");

            migrationBuilder.CreateIndex(
                name: "IX_UserVouchers_VoucherId",
                table: "UserVouchers",
                column: "VoucherId");

            migrationBuilder.CreateIndex(
                name: "UQ__UserVouc__73EF96E8102BBE1E",
                table: "UserVouchers",
                column: "MaSuDung",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_LoaiVoucher",
                table: "Vouchers",
                column: "LoaiVoucher");

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_OwnerId",
                table: "Vouchers",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_SanBongId",
                table: "Vouchers",
                column: "SanBongId");

            migrationBuilder.CreateIndex(
                name: "UQ__Vouchers__0AAC5B1029A0D8F8",
                table: "Vouchers",
                column: "MaVoucher",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DatSans_Voucher",
                table: "DatSans",
                column: "VoucherId",
                principalTable: "Vouchers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DatSans_Voucher",
                table: "DatSans");

            migrationBuilder.DropTable(
                name: "AnhSanBongs");

            migrationBuilder.DropTable(
                name: "DiemThuongLogs");

            migrationBuilder.DropTable(
                name: "OtpCodes");

            migrationBuilder.DropTable(
                name: "SanYeuThichs");

            migrationBuilder.DropTable(
                name: "UserVouchers");

            migrationBuilder.DropTable(
                name: "Vouchers");

            migrationBuilder.DropIndex(
                name: "IX_DatSans_VoucherId",
                table: "DatSans");

            migrationBuilder.DropColumn(
                name: "DaXacThucSdt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DiemHienTai",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LoaiVoucherApDung",
                table: "DatSans");

            migrationBuilder.DropColumn(
                name: "TienGiam",
                table: "DatSans");

            migrationBuilder.DropColumn(
                name: "TienGoc",
                table: "DatSans");

            migrationBuilder.DropColumn(
                name: "VoucherId",
                table: "DatSans");

            migrationBuilder.DropColumn(
                name: "SoSaoCoSoVatChat",
                table: "DanhGias");

            migrationBuilder.DropColumn(
                name: "SoSaoNhanVien",
                table: "DanhGias");
        }
    }
}
