using MakoIoT.Device.Services.Configuration.Metadata.Attributes;

namespace MakoIoT.Device.Services.WiFi.AP.Configuration
{
    [SectionMetadata("Wi-Fi Access Point")]
    public class WiFiAPConfig
    {
        [ParameterMetadata("SSID")] 
        public string Ssid { get; set; }

        [ParameterMetadata("Password", isSecret: true)]
        public string Password { get; set; }

        [ParameterMetadata("IP address", isHidden: true)]
        public string IpAddress { get; set; } = "192.168.4.1";

        [ParameterMetadata("Subnet mask", isHidden: true)]
        public string SubnetMask { get; set; } = "255.255.255.0";

        [ParameterMetadata("Max no. of connections", type: "int", isHidden: true)]
        public byte MaxConnections { get; set; } = 1;

        [ParameterMetadata("DHCP enabled", isHidden: true)]
        public bool EnableDhcp { get; set; } = true;

        public static string SectionName => "WiFiAP";
    }
}
