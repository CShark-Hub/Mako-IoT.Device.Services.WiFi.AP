using System;
using System.Device.Wifi;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using Iot.Device.DhcpServer;
using MakoIoT.Device.Services.Interface;
using MakoIoT.Device.Services.WiFi.AP.Configuration;
using MakoIoT.Device.Services.WiFi.AP.Model;
using MakoIoT.Device.Utilities.Invoker;
using Microsoft.Extensions.Logging;

namespace MakoIoT.Device.Services.WiFi.AP
{
    /// <inheritdoc />
    public class WiFiInterfaceManager : INetworkInterfaceManager
    {
        private readonly NetworkInterface _wifiInterface;
        private readonly NetworkInterface _apInterface;
        private readonly Wireless80211Configuration _wifiConfiguration;
        private readonly WirelessAPConfiguration _apConfiguration;
        private readonly WiFiAPConfig _config;
        private DhcpServer _dhcpServer;
        private readonly ILogger _logger;
        private AutoResetEvent _semaphore;
        private WiFiNetworkInfo[] _networks;

        public WiFiInterfaceManager(IConfigurationService configService, ILogger logger)
        {
            _logger = logger;
            _config = (WiFiAPConfig)configService.GetConfigSection(WiFiAPConfig.SectionName, typeof(WiFiAPConfig));

            FindInterfaces(out _wifiInterface, out _apInterface);
            _wifiConfiguration = Wireless80211Configuration.GetAllWireless80211Configurations()[_wifiInterface.SpecificConfigId];
            _apConfiguration = WirelessAPConfiguration.GetAllWirelessAPConfigurations()[_apInterface.SpecificConfigId];
        }

        /// <inheritdoc />
        public bool IsWifiEnabled => (_wifiConfiguration.Options & Wireless80211Configuration.ConfigurationOptions.Enable)
                                     == Wireless80211Configuration.ConfigurationOptions.Enable;

        /// <inheritdoc />
        public bool IsApEnabled => (_apConfiguration.Options & WirelessAPConfiguration.ConfigurationOptions.Enable)
                                   == WirelessAPConfiguration.ConfigurationOptions.Enable;

        /// <inheritdoc />
        public string WifiIpAddress => _wifiInterface.IPv4Address;
        
        /// <inheritdoc />
        public string ApIpAddress => _apInterface.IPv4Address;

        /// <inheritdoc />
        public bool HasPendingChanges { get; private set; }

        /// <inheritdoc />
        public void EnableWiFi()
        {
            _wifiConfiguration.Options = Wireless80211Configuration.ConfigurationOptions.Enable;
            _wifiConfiguration.SaveConfiguration();
            _logger.LogDebug("WiFi enabled");
            HasPendingChanges = true;
        }

        /// <inheritdoc />
        public void DisableWiFi()
        {
            _wifiConfiguration.Options = Wireless80211Configuration.ConfigurationOptions.Disable;
            _wifiConfiguration.SaveConfiguration();
            _logger.LogDebug("WiFi disabled");
            HasPendingChanges = true;
        }

        /// <inheritdoc />
        public void EnableAP()
        {
            _apInterface.EnableStaticIPv4(_config.IpAddress, _config.SubnetMask, _config.IpAddress);
            _apConfiguration.Ssid = _config.Ssid;
            if (String.IsNullOrEmpty(_config.Password))
                _apConfiguration.Authentication = AuthenticationType.Open;
            else
            {
                _apConfiguration.Authentication = AuthenticationType.WPA2;
                _apConfiguration.Password = _config.Password;
            }

            _apConfiguration.MaxConnections = _config.MaxConnections;

            _apConfiguration.Options = WirelessAPConfiguration.ConfigurationOptions.Enable |
                                       WirelessAPConfiguration.ConfigurationOptions.AutoStart;
            _apConfiguration.SaveConfiguration();

            _logger.LogDebug("AP enabled");

            HasPendingChanges = true;
        }

        /// <inheritdoc />
        public void DisableAP()
        {
            _apConfiguration.Options = WirelessAPConfiguration.ConfigurationOptions.Disable;
            _apConfiguration.SaveConfiguration();

            _logger.LogDebug("AP enabled");

            HasPendingChanges = true;
        }

        /// <inheritdoc />
        public void StartDhcp()
        {
            _dhcpServer ??= new DhcpServer { CaptivePortalUrl = $"http://{_config.IpAddress}" };

            Invoker.Retry(() =>
            {
                var dhcpInitResult =
                    _dhcpServer.Start(IPAddress.Parse(_config.IpAddress), IPAddress.Parse(_config.SubnetMask));
                if (!dhcpInitResult)
                    throw new Exception("DHCP failed to start");

                _logger.LogDebug($"DHCP started: {dhcpInitResult}");
            }, 3);
        }

        /// <inheritdoc />
        public void StopDhcp()
        {
            _dhcpServer?.Stop();
            _logger.LogDebug($"DHCP stopped");
        }

        /// <inheritdoc />
        public WiFiNetworkInfo[] GetAvailableNetworks()
        {
            _logger.LogTrace("GetAvailableNetworks()");

            _semaphore ??= new AutoResetEvent(false);
            _semaphore.Reset();

            var wifi = WifiAdapter.FindAllAdapters()[0];
            wifi.AvailableNetworksChanged += WifiOnAvailableNetworksChanged;
            _networks = new WiFiNetworkInfo[0];

            wifi.ScanAsync();
            _logger.LogTrace("Scanning, waiting");

            _semaphore.WaitOne(60000, false);

            wifi.AvailableNetworksChanged -= WifiOnAvailableNetworksChanged;
            
            _logger.LogTrace($"done, networks found: {_networks.Length}");
            
            return _networks;
        }

        /// <inheritdoc />
        public void DisconnectWifi()
        {
            _logger.LogDebug("Wifi disconnecting");
            var adapters = WifiAdapter.FindAllAdapters();
            adapters[0].Disconnect();
            _logger.LogTrace($"{_wifiInterface.IPv4Address}");

        }

        private void WifiOnAvailableNetworksChanged(WifiAdapter sender, object e)
        {
            var networkReport = sender.NetworkReport;

            _logger.LogTrace($"AvailableNetworksChanged, AvailableNetworks.Length: {networkReport.AvailableNetworks.Length}");

            if (networkReport.AvailableNetworks.Length == 0)
                return;

            _networks = new WiFiNetworkInfo[networkReport.AvailableNetworks.Length];
            for (int i = 0; i < networkReport.AvailableNetworks.Length; i++)
            {
                _networks[i] = new WiFiNetworkInfo
                {
                    Bsid = networkReport.AvailableNetworks[i].Bsid,
                    Rssi = networkReport.AvailableNetworks[i].NetworkRssiInDecibelMilliwatts,
                    SignalBars = networkReport.AvailableNetworks[i].SignalBars,
                    Ssid = networkReport.AvailableNetworks[i].Ssid
                };
            }

            _logger.LogTrace($"networks count {_networks.Length}");

            if (_networks.Length > 0)
            {
                _logger.LogTrace("semaphore set");

                _semaphore.Set();
            }
        }

        private void FindInterfaces(out NetworkInterface wifiInterface, out NetworkInterface apInterface)
        {
            wifiInterface = null;
            apInterface = null;
            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                switch (networkInterface.NetworkInterfaceType)
                {
                    case NetworkInterfaceType.Wireless80211:
                        wifiInterface = networkInterface;
                        break;
                    case NetworkInterfaceType.WirelessAP:
                        apInterface = networkInterface;
                        break;
                }
            }
        }
    }
}
