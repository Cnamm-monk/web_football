using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web_Stadium.Migrations
{
    /// <inheritdoc />
    public partial class AddChuyenNhuongSoDienThoai : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SoDienThoaiLienHe",
                table: "ChuyenNhuongs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SoDienThoaiLienHe",
                table: "ChuyenNhuongs");
        }
    }
}
