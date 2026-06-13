using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web_Stadium.Migrations
{
    /// <inheritdoc />
    public partial class AddYeuCauDoiGio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "YeuCauDoiGios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatSanId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    KhungGioMoiId = table.Column<int>(type: "int", nullable: false),
                    NgayMoi = table.Column<DateTime>(type: "datetime", nullable: false),
                    LyDo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "ChoXuLy"),
                    GhiChuStaff = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GhiChuOwner = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GhiChuAdmin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StaffXuLyId = table.Column<int>(type: "int", nullable: true),
                    OwnerXuLyId = table.Column<int>(type: "int", nullable: true),
                    AdminXuLyId = table.Column<int>(type: "int", nullable: true),
                    ThoiGianTao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    ThoiGianXuLy = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YeuCauDoiGios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YeuCauDoiGios_DatSans_DatSanId",
                        column: x => x.DatSanId,
                        principalTable: "DatSans",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_YeuCauDoiGios_KhungGios_KhungGioMoiId",
                        column: x => x.KhungGioMoiId,
                        principalTable: "KhungGios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_YeuCauDoiGios_Users_AdminXuLyId",
                        column: x => x.AdminXuLyId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_YeuCauDoiGios_Users_OwnerXuLyId",
                        column: x => x.OwnerXuLyId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_YeuCauDoiGios_Users_StaffXuLyId",
                        column: x => x.StaffXuLyId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_YeuCauDoiGios_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauDoiGios_AdminXuLyId",
                table: "YeuCauDoiGios",
                column: "AdminXuLyId");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauDoiGios_DatSanId",
                table: "YeuCauDoiGios",
                column: "DatSanId");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauDoiGios_KhungGioMoiId",
                table: "YeuCauDoiGios",
                column: "KhungGioMoiId");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauDoiGios_OwnerXuLyId",
                table: "YeuCauDoiGios",
                column: "OwnerXuLyId");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauDoiGios_StaffXuLyId",
                table: "YeuCauDoiGios",
                column: "StaffXuLyId");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauDoiGios_UserId",
                table: "YeuCauDoiGios",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "YeuCauDoiGios");
        }
    }
}
