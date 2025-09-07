namespace BLBM_ENV.Models
{
    public class UnifiedPortViewModel
    {
        public string Port { get; set; }
        public int DetailID { get; set; }
        public int ConnectionID { get; set; }
        public string Type { get; set; }

        // --- YENİ EKLENDİ ---
        public string LinkStatus { get; set; }
        public string LinkSpeed { get; set; }
        // --- YENİ EKLENDİ SONU ---

        public string MacAddress { get; set; }
        public string WWPN { get; set; }
        public bool IsConnected { get; set; }
        public Envanter RemoteDevice { get; set; }
        public string RemotePort { get; set; }
        public bool IsVirtual { get; set; }
        public List<Baglanti> PassthroughConnections { get; set; } = new List<Baglanti>();
    }
}