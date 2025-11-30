using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BLBM_ENV.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(50)] // Optimize edildi: İsim için 50 yeterli
        public string? FirstName { get; set; }

        [StringLength(50)] // Optimize edildi: Soyisim için 50 yeterli
        public string? LastName { get; set; }

        // Bu veritabanına kaydedilmez, hesaplama alanıdır.
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}