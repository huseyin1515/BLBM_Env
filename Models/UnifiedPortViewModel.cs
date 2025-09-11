namespace BLBM_ENV.Models
{
    public class UnifiedPortViewModel
    {
        public string Port { get; set; }
        public int ConnectionID { get; set; }
        public string Type { get; set; }
        public string? LinkStatus { get; set; }
        public string? LinkSpeed { get; set; }

        // --- GÜNCELLENDİ: MAC Adresleri ayrıldı ---
        public string? BakirMAC { get; set; }
        public string? FiberMAC { get; set; }
        public string? WWPN { get; set; }

        public bool IsConnected { get; set; }
        public Envanter? RemoteDevice { get; set; }
        public string? RemotePort { get; set; }
        public bool IsVirtual { get; set; }
        public List<Baglanti> PassthroughConnections { get; set; } = new List<Baglanti>();
    }
}