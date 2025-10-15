using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace BLBM_ENV.Models
{
    public class Envanter
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "Cihaz Adı alanı zorunludur.")]
        [StringLength(256)]
        public string DeviceName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tür alanı zorunludur.")]
        [StringLength(50)]
        public string Tur { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ServiceTagSerialNumber { get; set; }

        [StringLength(150)]
        public string? Model { get; set; }

        [StringLength(100)]
        public string? IpAddress { get; set; }

        [StringLength(100)]
        public string? VcenterAddress { get; set; }

        [StringLength(100)]
        public string? ClusterName { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        [StringLength(150)]
        public string? OperatingSystem { get; set; }

        [StringLength(100)]
        public string? IloIdracIp { get; set; }

        [StringLength(50)]
        public string? Kabin { get; set; }

        [StringLength(50)]
        public string? RearFront { get; set; }

        [StringLength(50)]
        public string? KabinU { get; set; }

        public virtual ICollection<Baglanti> AsSourceConnections { get; set; } = new List<Baglanti>();
        public virtual ICollection<Baglanti> AsTargetConnections { get; set; } = new List<Baglanti>();
    }
}