using System.ComponentModel.DataAnnotations;

// Namespace'i projenizin ana adıyla tam olarak aynı yapıyoruz
namespace BLBM_ENV.Models
{
    public class Vip
    {
        [Key]
        public int ID { get; set; }
        public string? Dns { get; set; }
        public string? VipIp { get; set; }
        public string? Port { get; set; }
        public string? MakineIp { get; set; }
        public string? MakineAdi { get; set; }
        public string? Durumu { get; set; }
        public string? Network { get; set; }
        public string? Cluster { get; set; }
        public string? Host { get; set; }
        public string? OS { get; set; }
    }
}