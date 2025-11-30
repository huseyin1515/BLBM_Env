using BLBM_ENV.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BLBM_ENV.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<Envanter> Envanterler { get; set; }
        public DbSet<Baglanti> Baglantilar { get; set; }
        public DbSet<Vip> Vips { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        // Veritabanı Optimizasyonu: Tüm stringleri 'varchar' (ANSI) yapar
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<string>().AreUnicode(false);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- IDENTITY TABLOLARINI OPTİMİZE ETME ---
            modelBuilder.Entity<ApplicationUser>(b =>
            {
                b.Property(u => u.Id).HasMaxLength(50);
                b.Property(u => u.ConcurrencyStamp).HasMaxLength(50);
                b.Property(u => u.PasswordHash).HasMaxLength(256);
                b.Property(u => u.SecurityStamp).HasMaxLength(50);
                b.Property(u => u.PhoneNumber).HasMaxLength(20);
            });

            modelBuilder.Entity<IdentityRole>(b =>
            {
                b.Property(r => r.Id).HasMaxLength(50);
                b.Property(r => r.ConcurrencyStamp).HasMaxLength(50);
            });

            modelBuilder.Entity<IdentityUserRole<string>>(b =>
            {
                b.Property(ur => ur.UserId).HasMaxLength(50);
                b.Property(ur => ur.RoleId).HasMaxLength(50);
            });

            modelBuilder.Entity<IdentityUserClaim<string>>(b => b.Property(c => c.UserId).HasMaxLength(50));
            modelBuilder.Entity<IdentityRoleClaim<string>>(b => b.Property(c => c.RoleId).HasMaxLength(50));

            modelBuilder.Entity<IdentityUserLogin<string>>(b =>
            {
                b.Property(l => l.LoginProvider).HasMaxLength(128);
                b.Property(l => l.ProviderKey).HasMaxLength(128);
                b.Property(l => l.UserId).HasMaxLength(50);
            });

            modelBuilder.Entity<IdentityUserToken<string>>(b =>
            {
                b.Property(t => t.UserId).HasMaxLength(50);
                b.Property(t => t.LoginProvider).HasMaxLength(128);
                b.Property(t => t.Name).HasMaxLength(128);
            });

            // --- BAĞLANTI İLİŞKİLERİ ---
            // SQL Server'da "Multiple Cascade Paths" hatasını önlemek için Restrict kullanıyoruz.
            // Silme işlemini Controller tarafında kod ile yönetiyoruz.

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