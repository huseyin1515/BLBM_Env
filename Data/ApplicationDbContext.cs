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
        public DbSet<EnvanterDetail> EnvanterDetails { get; set; }
        public DbSet<Vip> Vips { get; set; }


        // Bu metot, Entity Framework'ün veritabanı modelini oluştururken çalışır
        // ve bizim özel kurallarımızı uygulamamızı sağlar.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // Baglanti tablosundaki SourceDeviceID ilişkisi için kural tanımlaması
            modelBuilder.Entity<Baglanti>()
                .HasOne(b => b.SourceDevice) // Bir bağlantının bir kaynak cihazı vardır
                .WithMany(e => e.AsSourceConnections) // Bir envanterin birden çok kaynak bağlantısı olabilir
                .HasForeignKey(b => b.SourceDeviceID) // Yabancı anahtar SourceDeviceID'dir
                .OnDelete(DeleteBehavior.Restrict); // Envanter silinirse, bu bağlantıyı silme, işlemi engelle.


            // Baglanti tablosundaki TargetDeviceID ilişkisi için kural tanımlaması
            modelBuilder.Entity<Baglanti>()
                .HasOne(b => b.TargetDevice) // Bir bağlantının bir hedef cihazı vardır
                .WithMany(e => e.AsTargetConnections) // Bir envanterin birden çok hedef bağlantısı olabilir
                .HasForeignKey(b => b.TargetDeviceID) // Yabancı anahtar TargetDeviceID'dir
                .OnDelete(DeleteBehavior.Restrict); // Envanter silinirse, bu bağlantıyı silme, işlemi engelle.
        }
    }
}