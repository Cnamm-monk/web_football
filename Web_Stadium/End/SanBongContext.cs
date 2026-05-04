using Microsoft.EntityFrameworkCore;

namespace Web_Stadium.End
{
    public class SanBongContext : DbContext
    {
        public SanBongContext(DbContextOptions<SanBongContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<SanBong> SanBongs { get; set; }
        public DbSet<DatSan> DatSans { get; set; }
        public DbSet<DatSan_DichVu> DatSan_DichVus { get; set; }
        public DbSet<DichVu> DichVus { get; set; }
        public DbSet<KhungGio> KhungGios { get; set; }
        public DbSet<DanhGia> DanhGias { get; set; }
        public DbSet<Matchmaking> Matchmakings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tắt cascade delete cho toàn bộ project 
            foreach (var relationship in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.NoAction;
            }
        }
    }
}