using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web_Stadium.Migrations
{
    /// <inheritdoc />
    public partial class AddGhepTran : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GhepTrans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatSanAId = table.Column<int>(type: "int", nullable: false),
                    DatSanBId = table.Column<int>(type: "int", nullable: true),
                    UserAId = table.Column<int>(type: "int", nullable: false),
                    UserBId = table.Column<int>(type: "int", nullable: true),
                    LoiNhan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SanChonId = table.Column<int>(type: "int", nullable: true),
                    KhungGioChonId = table.Column<int>(type: "int", nullable: true),
                    HinhThuc = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "ChoXacNhan"),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThoiGianTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianXuLy = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GhepTrans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GhepTrans_DatSans_DatSanAId",
                        column: x => x.DatSanAId,
                        principalTable: "DatSans",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GhepTrans_DatSans_DatSanBId",
                        column: x => x.DatSanBId,
                        principalTable: "DatSans",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GhepTrans_KhungGios_KhungGioChonId",
                        column: x => x.KhungGioChonId,
                        principalTable: "KhungGios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GhepTrans_SanBongs_SanChonId",
                        column: x => x.SanChonId,
                        principalTable: "SanBongs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GhepTrans_Users_UserAId",
                        column: x => x.UserAId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GhepTrans_Users_UserBId",
                        column: x => x.UserBId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GhepTrans_DatSanAId",
                table: "GhepTrans",
                column: "DatSanAId");

            migrationBuilder.CreateIndex(
                name: "IX_GhepTrans_DatSanBId",
                table: "GhepTrans",
                column: "DatSanBId");

            migrationBuilder.CreateIndex(
                name: "IX_GhepTrans_KhungGioChonId",
                table: "GhepTrans",
                column: "KhungGioChonId");

            migrationBuilder.CreateIndex(
                name: "IX_GhepTrans_SanChonId",
                table: "GhepTrans",
                column: "SanChonId");

            migrationBuilder.CreateIndex(
                name: "IX_GhepTrans_UserAId",
                table: "GhepTrans",
                column: "UserAId");

            migrationBuilder.CreateIndex(
                name: "IX_GhepTrans_UserBId",
                table: "GhepTrans",
                column: "UserBId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GhepTrans");
        }
    }
}
