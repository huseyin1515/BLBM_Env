using System.ComponentModel.DataAnnotations;

namespace BLBM_ENV.Models
{
    public class Vip
    {
        [Key]
        public int ID { get; set; }

        [StringLength(256)]
        public string? Dns { get; set; }

        [StringLength(100)]
        public string? VipIp { get; set; }

        [StringLength(50)]
        public string? Port { get; set; }

        [StringLength(100)]
        public string? MakineIp { get; set; }

        [StringLength(256)]
        public string? MakineAdi { get; set; }

        [StringLength(50)]
        public string? Durumu { get; set; }

        [StringLength(150)]
        public string? Network { get; set; }

        [StringLength(150)]
        public string? Cluster { get; set; }

        [StringLength(256)]
        public string? Host { get; set; }

        [StringLength(150)]
        public string? OS { get; set; }
    }
}