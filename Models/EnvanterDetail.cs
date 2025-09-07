using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BLBM_ENV.Models
{
    public class EnvanterDetail
    {
        [Key]
        public int ID { get; set; }

        public int EnvanterID { get; set; }

        // Kaynak Cihaz Bilgileri (Otomatik Doldurulacak)
        public string? Turu { get; set; }
        public string? DeviceName { get; set; }
        public string? DeviceServiceTag { get; set; }
        public string? DeviceModel { get; set; }

        // Kullanıcının Gireceği Alanlar
        public string? Lok { get; set; }
        public string? LinkStatus { get; set; }
        public string? LinkSpeed { get; set; }
        public string? PortID { get; set; }
        public string? NicID { get; set; }
        public string? FiberMAC { get; set; }
        public string? BakirMAC { get; set; }
        public string? WWPN { get; set; }
        public string? SwName { get; set; }
        public string? SwPort { get; set; }

        // --- YENİ EKLENEN ALANLAR ---
        // Hedef Switch Bilgileri (Otomatik Doldurulacak)
        [Display(Name = "Switch Modeli")]
        public string? SwModel { get; set; }

        [Display(Name = "Switch Servis Etiketi")]
        public string? SwServiceTag { get; set; }

        [ForeignKey("EnvanterID")]
        public virtual Envanter? Envanter { get; set; }
    }
}