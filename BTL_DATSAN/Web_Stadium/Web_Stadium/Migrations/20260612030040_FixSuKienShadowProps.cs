using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web_Stadium.Migrations
{
    /// <inheritdoc />
    public partial class FixSuKienShadowProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Shadow properties DoiBongId / ThanhVienDoiId were never applied to the DB
            // (table created via SanBongBTL.sql before EF migrations).
            // Fix: WithMany(d => d.SuKiens) wires inverse nav, removing shadow FKs.
            // No DB changes needed — migration only syncs the model snapshot.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
