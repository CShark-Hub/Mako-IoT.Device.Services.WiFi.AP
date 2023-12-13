namespace MakoIoT.Device.Services.WiFi.AP.Configuration
{
    public class WiFiAPConfig
    {
        public string Ssid { get; set; }

        public string Password { get; set; }

        public string IpAddress { get; set; } = "192.168.4.1";

        public string SubnetMask { get; set; } = "255.255.255.0";

        public byte MaxConnections { get; set; } = 1;

        public bool EnableDhcp { get; set; } = true;

        public static string SectionName => "WiFiAP";
    }
}
