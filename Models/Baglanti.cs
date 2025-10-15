using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BLBM_ENV.Models
{
    public class Baglanti
    {
        [Key]
        public int ID { get; set; }

        public int? SourceDeviceID { get; set; }
        public int? TargetDeviceID { get; set; }

        [StringLength(100)]
        public string Source_Port { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Source_LinkStatus { get; set; }

        [StringLength(50)]
        public string? Source_LinkSpeed { get; set; }

        [StringLength(100)]
        public string? Source_NicID { get; set; }

        [StringLength(50)]
        public string? Source_FiberMAC { get; set; }

        [StringLength(50)]
        public string? Source_BakirMAC { get; set; }

        [StringLength(50)]
        public string? Source_WWPN { get; set; }

        [StringLength(100)]
        public string Target_Port { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Target_LinkStatus { get; set; }

        [StringLength(50)]
        public string? Target_LinkSpeed { get; set; }

        [StringLength(50)]
        public string? Target_FiberMAC { get; set; }

        [StringLength(50)]
        public string? Target_BakirMAC { get; set; }

        [StringLength(50)]
        public string? Target_WWPN { get; set; }

        [StringLength(100)]
        public string? ConnectionType { get; set; }

        [ForeignKey("SourceDeviceID")]
        [InverseProperty("AsSourceConnections")]
        public virtual Envanter? SourceDevice { get; set; }

        [ForeignKey("TargetDeviceID")]
        [InverseProperty("AsTargetConnections")]
        public virtual Envanter? TargetDevice { get; set; }
    }
}