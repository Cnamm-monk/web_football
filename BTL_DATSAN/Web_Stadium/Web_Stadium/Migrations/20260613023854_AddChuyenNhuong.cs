using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web_Stadium.Migrations
{
    /// <inheritdoc />
    public partial class AddChuyenNhuong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChuyenNhuongs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatSanId = table.Column<int>(type: "int", nullable: false),
                    UserAId = table.Column<int>(type: "int", nullable: false),
                    UserBId = table.Column<int>(type: "int", nullable: true),
                    TieuDe = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GiaChuyenNhuong = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "DangTim"),
                    GhiChuStaff = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GhiChuOwner = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StaffXuLyId = table.Column<int>(type: "int", nullable: true),
                    OwnerXuLyId = table.Column<int>(type: "int", nullable: true),
                    DaChuyenNhuong = table.Column<bool>(type: "bit", nullable: false),
                    ThoiGianTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianXuLy = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuyenNhuongs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChuyenNhuongs_DatSans_DatSanId",
                        column: x => x.DatSanId,
                        principalTable: "DatSans",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChuyenNhuongs_Users_OwnerXuLyId",
                        column: x => x.OwnerXuLyId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChuyenNhuongs_Users_StaffXuLyId",
                        column: x => x.StaffXuLyId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChuyenNhuongs_Users_UserAId",
                        column: x => x.UserAId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChuyenNhuongs_Users_UserBId",
                        column: x => x.UserBId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChuyenNhuongs_DatSanId",
                table: "ChuyenNhuongs",
                column: "DatSanId");

            migrationBuilder.CreateIndex(
                name: "IX_ChuyenNhuongs_OwnerXuLyId",
                table: "ChuyenNhuongs",
                column: "OwnerXuLyId");

            migrationBuilder.CreateIndex(
                name: "IX_ChuyenNhuongs_StaffXuLyId",
                table: "ChuyenNhuongs",
                column: "StaffXuLyId");

            migrationBuilder.CreateIndex(
                name: "IX_ChuyenNhuongs_UserAId",
                table: "ChuyenNhuongs",
                column: "UserAId");

            migrationBuilder.CreateIndex(
                name: "IX_ChuyenNhuongs_UserBId",
                table: "ChuyenNhuongs",
                column: "UserBId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChuyenNhuongs");
        }
    }
}
