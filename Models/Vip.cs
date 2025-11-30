using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore; // Index için gerekli

namespace BLBM_ENV.Models
{
    // Sık aranan alanlara Index atıyoruz
    [Index(nameof(Dns))]
    [Index(nameof(VipIp))]
    public class Vip
    {
        [Key]
        public int ID { get; set; }

        [StringLength(100)] // DNS isimleri
        public string? Dns { get; set; }

        [StringLength(50)] // IP
        public string? VipIp { get; set; }

        [StringLength(20)] // Port numarası
        public string? Port { get; set; }

        [StringLength(50)]
        public string? MakineIp { get; set; }

        [StringLength(100)]
        public string? MakineAdi { get; set; }

        [StringLength(20)] // Up/Down
        public string? Durumu { get; set; }

        [StringLength(50)] // Network
        public string? Network { get; set; }

        [StringLength(50)] // Cluster
        public string? Cluster { get; set; }

        [StringLength(100)] // Host
        public string? Host { get; set; }

        [StringLength(60)] // OS (İşletim Sistemi Adı)
        public string? OS { get; set; }

        // Eşzamanlılık (Concurrency) Kilidi
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}