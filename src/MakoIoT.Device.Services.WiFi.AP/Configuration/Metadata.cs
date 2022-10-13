namespace MakoIoT.Device.Services.WiFi.AP.Configuration
{
    public static class Metadata
    {
        public static string WiFiAPConfig => @"{""Name"":""WiFiAP"",""Label"":""Wi-Fi Access Point"",""IsHidden"":false,""Parameters"":[{""Name"":""Ssid"",""Type"":""string"",""Label"":""SSID"",""IsHidden"":false,""IsSecret"":false,""DefaultValue"":null},{""Name"":""Password"",""Type"":""string"",""Label"":""Password"",""IsHidden"":false,""IsSecret"":true,""DefaultValue"":null},{""Name"":""IpAddress"",""Type"":""string"",""Label"":""IP address"",""IsHidden"":true,""IsSecret"":false,""DefaultValue"":null},{""Name"":""SubnetMask"",""Type"":""string"",""Label"":""Subnet mask"",""IsHidden"":true,""IsSecret"":false,""DefaultValue"":null},{""Name"":""MaxConnections"",""Type"":""int"",""Label"":""Max no. of connections"",""IsHidden"":true,""IsSecret"":false,""DefaultValue"":null},{""Name"":""EnableDhcp"",""Type"":""bool"",""Label"":""DHCP enabled"",""IsHidden"":true,""IsSecret"":false,""DefaultValue"":null}]}";
    }
}
