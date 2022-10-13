namespace MakoIoT.Device.Services.WiFi.AP.Model
{
    public class WiFiNetworkInfo
    {
        public string Ssid { get; set; }
        public string Bsid { get; set; }
        public double Rssi { get; set; }
        public byte SignalBars { get; set; }
    }
}
