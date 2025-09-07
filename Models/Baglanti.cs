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

        // Kaynak Port Detayları
        public string Source_Port { get; set; } = string.Empty;
        public string? Source_LinkStatus { get; set; }
        public string? Source_LinkSpeed { get; set; }
        public string? Source_NicID { get; set; }
        public string? Source_FiberMAC { get; set; }
        public string? Source_BakirMAC { get; set; }
        public string? Source_WWPN { get; set; }

        // Hedef Port Detayları
        public string Target_Port { get; set; } = string.Empty;
        // --- YENİ EKLENDİ: Hedef port detayları ---
        public string? Target_LinkStatus { get; set; }
        public string? Target_LinkSpeed { get; set; }
        public string? Target_FiberMAC { get; set; }
        public string? Target_BakirMAC { get; set; }
        public string? Target_WWPN { get; set; }

        public string? ConnectionType { get; set; }

        [ForeignKey("SourceDeviceID")]
        [InverseProperty("AsSourceConnections")]
        public virtual Envanter? SourceDevice { get; set; }

        [ForeignKey("TargetDeviceID")]
        [InverseProperty("AsTargetConnections")]
        public virtual Envanter? TargetDevice { get; set; }
    }
}