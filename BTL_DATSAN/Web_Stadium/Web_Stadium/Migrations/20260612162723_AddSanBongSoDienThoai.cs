using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web_Stadium.Migrations
{
    /// <inheritdoc />
    public partial class AddSanBongSoDienThoai : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SoDienThoai",
                table: "SanBongs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SoDienThoai",
                table: "SanBongs");
        }
    }
}
