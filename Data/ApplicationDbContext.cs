using BLBM_ENV.Models;
using Microsoft.EntityFrameworkCore;

namespace BLBM_ENV.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Envanter> Envanterler { get; set; }
        public DbSet<Baglanti> Baglantilar { get; set; }
        // --- KALDIRILDI: EnvanterDetails DbSet'i artık yok ---
        // public DbSet<EnvanterDetail> EnvanterDetails { get; set; }
        public DbSet<Vip> Vips { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Baglanti>()
                .HasOne(b => b.SourceDevice)
                .WithMany(e => e.AsSourceConnections)
                .HasForeignKey(b => b.SourceDeviceID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Baglanti>()
                .HasOne(b => b.TargetDevice)
                .WithMany(e => e.AsTargetConnections)
                .HasForeignKey(b => b.TargetDeviceID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}