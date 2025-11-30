namespace BLBM_ENV.Models
{
    public class DashboardViewModel
    {
        // Üst Kısımdaki Kartlar İçin Sayılar
        public int TotalDevices { get; set; }        // Toplam Cihaz
        public int TotalConnections { get; set; }    // Toplam Bağlantı
        public int TotalVips { get; set; }           // Toplam VIP
        public int TotalServers { get; set; }        // Sadece Sunucular

        // Grafikler İçin Veriler (Key: Kategori Adı, Value: Sayı)
        public Dictionary<string, int> DeviceByTypes { get; set; } // Cihaz Türü Dağılımı (Server, Switch vb.)
        public Dictionary<string, int> DeviceByOS { get; set; }    // İşletim Sistemi Dağılımı
        public Dictionary<string, int> ConnectionTypes { get; set; } // Bağlantı Türü Dağılımı (FC, Bakır)

        // Tablo İçin Son Eklenenler
        public List<Envanter> LastAddedDevices { get; set; }
    }
}