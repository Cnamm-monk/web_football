using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web_Stadium.Migrations
{
    /// <inheritdoc />
    public partial class AddYeuCauDoiSan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "YeuCauDoiSans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatSanId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SanMoiId = table.Column<int>(type: "int", nullable: false),
                    KhungGioMoiId = table.Column<int>(type: "int", nullable: false),
                    NgayThiDau = table.Column<DateTime>(type: "datetime", nullable: false),
                    LyDo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "ChoXuLy"),
                    ChenhLechGia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GhiChuOwner = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OwnerXuLyId = table.Column<int>(type: "int", nullable: true),
                    ThoiGianTao = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    ThoiGianXuLy = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YeuCauDoiSans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YeuCauDoiSans_DatSans_DatSanId",
                        column: x => x.DatSanId,
                        principalTable: "DatSans",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_YeuCauDoiSans_KhungGios_KhungGioMoiId",
                        column: x => x.KhungGioMoiId,
                        principalTable: "KhungGios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_YeuCauDoiSans_SanBongs_SanMoiId",
                        column: x => x.SanMoiId,
                        principalTable: "SanBongs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_YeuCauDoiSans_Users_OwnerXuLyId",
                        column: x => x.OwnerXuLyId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_YeuCauDoiSans_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauDoiSans_DatSanId",
                table: "YeuCauDoiSans",
                column: "DatSanId");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauDoiSans_KhungGioMoiId",
                table: "YeuCauDoiSans",
                column: "KhungGioMoiId");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauDoiSans_OwnerXuLyId",
                table: "YeuCauDoiSans",
                column: "OwnerXuLyId");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauDoiSans_SanMoiId",
                table: "YeuCauDoiSans",
                column: "SanMoiId");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauDoiSans_UserId",
                table: "YeuCauDoiSans",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "YeuCauDoiSans");
        }
    }
}
