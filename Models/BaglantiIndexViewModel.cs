namespace BLBM_ENV.Models
{
    public class BaglantiIndexViewModel
    {
        public int BaglantiID { get; set; }


        public int? SourceDeviceID { get; set; }
        public string? SourceDeviceName { get; set; }
        public string? SourcePort { get; set; }


        public int? TargetDeviceID { get; set; }
        public string? TargetDeviceName { get; set; }
        public string? TargetPort { get; set; }


        public string? ConnectionType { get; set; }


        public string? LinkStatus { get; set; }
        public string? LinkSpeed { get; set; }
        public string? NicID { get; set; }
        public string? BakirMAC { get; set; }
        public string? FiberMAC { get; set; }
        public string? WWPN { get; set; }
    }
}