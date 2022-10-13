using MakoIoT.Device.Services.WiFi.AP.Model;

namespace MakoIoT.Device.Services.WiFi.AP
{
    /// <summary>
    /// Manages network interfaces.
    /// </summary>
    public interface INetworkInterfaceManager
    {
        /// <summary>
        /// True, if Wi-Fi interface is enabled.
        /// </summary>
        bool IsWifiEnabled { get; }

        /// <summary>
        /// True, if Access Point interface is enabled.
        /// </summary>
        bool IsApEnabled { get; }

        /// <summary>
        /// Wi-Fi interface IP address
        /// </summary>
        string WifiIpAddress { get; }

        /// <summary>
        /// Access Point IP address
        /// </summary>
        string ApIpAddress { get; }

        /// <summary>
        /// Enables Wi-Fi interface.
        /// </summary>
        void EnableWiFi();

        /// <summary>
        /// Disables Wi-Fi interface.
        /// </summary>
        void DisableWiFi();

        /// <summary>
        /// Enables Access Point interface.
        /// </summary>
        void EnableAP();

        /// <summary>
        /// Enables Access Point interface.
        /// </summary>
        void DisableAP();

        /// <summary>
        /// Gets available Wi-Fi networks in range.
        /// </summary>
        /// <returns>List of networks</returns>
        WiFiNetworkInfo[] GetAvailableNetworks();

        /// <summary>
        /// Disconnects Wi-Fi interface from any network.
        /// </summary>
        void DisconnectWifi();

        /// <summary>
        /// Starts DHCP server.
        /// </summary>
        void StartDhcp();

        /// <summary>
        /// Stops DHCP server.
        /// </summary>
        void StopDhcp();

        /// <summary>
        /// True, if changes have been made that require device reboot.
        /// </summary>
        bool HasPendingChanges { get; }
    }
}
