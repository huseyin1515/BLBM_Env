using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BLBM_ENV.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50)] // Email yeterli (admin@belbim...)
        public string UserEmail { get; set; }

        [StringLength(50)] // Ad Soyad
        public string UserName { get; set; }

        [StringLength(20)] // Ekleme, Silme
        public string Action { get; set; }

        [StringLength(20)] // Envanter, Vip
        public string Module { get; set; }

        [StringLength(250)] // Açıklama için yeterli
        public string Description { get; set; }

        // JSON verisi sınırsız olmalı
        public string? OldValues { get; set; }

        // datetime2(0) saniye hassasiyetidir (Milisaniye tutmaz, yer kazandırır)
        [Column(TypeName = "datetime2(0)")]
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}