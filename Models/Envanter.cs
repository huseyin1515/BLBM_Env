using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace BLBM_ENV.Models
{
    public class Envanter
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Cihaz Adı alanı zorunludur.")]
        public string DeviceName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tür alanı zorunludur.")]
        public string Tur { get; set; } = string.Empty;

        public string? ServiceTagSerialNumber { get; set; }
        public string? Model { get; set; }
        public string? IpAddress { get; set; }
        public string? VcenterAddress { get; set; }
        public string? ClusterName { get; set; }
        public string? Location { get; set; }
        public string? OperatingSystem { get; set; }
        public string? IloIdracIp { get; set; }
        public string? Kabin { get; set; }
        public string? RearFront { get; set; }
        public string? KabinU { get; set; }

        public virtual ICollection<Baglanti> AsSourceConnections { get; set; } = new List<Baglanti>();
        public virtual ICollection<Baglanti> AsTargetConnections { get; set; } = new List<Baglanti>();

        // --- YENİ EKLENDİ: Envanterin port detaylarına erişim için ---
        public virtual ICollection<EnvanterDetail> Details { get; set; } = new List<EnvanterDetail>();
    }
}