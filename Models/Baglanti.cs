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


        public string? Source_DeviceName { get; set; }
        public string? Source_Tur { get; set; }
        public string? Source_Model { get; set; }
        public string? Source_ServiceTag { get; set; }
        public string? Source_IpAddress { get; set; }
        public string Source_Port { get; set; } = string.Empty;


        public string? Target_DeviceName { get; set; }
        public string? Target_Tur { get; set; }
        public string? Target_Model { get; set; }
        public string? Target_ServiceTag { get; set; }
        public string? Target_IpAddress { get; set; }
        public string Target_Port { get; set; } = string.Empty;


        public string? ConnectionType { get; set; }
        public string? LinkStatus { get; set; }
        public string? LinkSpeed { get; set; }


        [ForeignKey("SourceDeviceID")]
        [InverseProperty("AsSourceConnections")]
        public virtual Envanter? SourceDevice { get; set; }


        [ForeignKey("TargetDeviceID")]
        [InverseProperty("AsTargetConnections")]
        public virtual Envanter? TargetDevice { get; set; }
    }
}