using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore; // Index için gerekli

namespace BLBM_ENV.Models
{
    // Sık aranan alanlara Index atıyoruz
    [Index(nameof(DeviceName))]
    [Index(nameof(IpAddress))]
    [Index(nameof(ServiceTagSerialNumber))]
    public class Envanter
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Cihaz Adı zorunludur.")]
        [StringLength(60)] // 100 -> 60 (Yeterli)
        public string DeviceName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tür zorunludur.")]
        [StringLength(20)] // 30 -> 20 (Server, Switch...)
        public string Tur { get; set; } = string.Empty;

        // IPv4: Max 15 karakter. IPv6 payı ile 45. 
        [StringLength(45)]
        public string? IpAddress { get; set; }

        [StringLength(45)]
        public string? IloIdracIp { get; set; }

        [StringLength(30)] // 50 -> 30 (Seri nolar genelde kısadır)
        public string? ServiceTagSerialNumber { get; set; }

        [StringLength(60)] // 100 -> 60
        public string? Model { get; set; }

        [StringLength(45)]
        public string? VcenterAddress { get; set; }

        [StringLength(50)]
        public string? ClusterName { get; set; }

        [StringLength(30)] // Lokasyon kodları kısadır
        public string? Location { get; set; }

        [StringLength(50)] // OS isimleri (Windows Server 2022...)
        public string? OperatingSystem { get; set; }

        [StringLength(20)] // A1, B2...
        public string? Kabin { get; set; }

        [StringLength(10)] // Front/Rear (5 karakter)
        public string? RearFront { get; set; }

        [StringLength(10)] // 42, 10-12
        public string? KabinU { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public virtual ICollection<Baglanti> AsSourceConnections { get; set; } = new List<Baglanti>();
        public virtual ICollection<Baglanti> AsTargetConnections { get; set; } = new List<Baglanti>();
    }
}