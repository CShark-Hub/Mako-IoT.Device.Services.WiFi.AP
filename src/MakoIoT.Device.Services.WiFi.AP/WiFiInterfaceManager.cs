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

namespace MakoIoT.Device.Services.WiFi.AP
{
    /// <inheritdoc />
    public class WiFiInterfaceManager : INetworkInterfaceManager
    {
        private NetworkInterface _wifiInterface;
        private NetworkInterface wifiInterface
        {
            get
            {
                if (_wifiInterface == null)
                {
                    _wifiInterface = FindInterfaces(NetworkInterfaceType.Wireless80211);
                }

                return _wifiInterface;
            }
        }

        private NetworkInterface _apInterface;
        private NetworkInterface apInterface
        {
            get
            {
                if (_apInterface == null)
                {
                    _apInterface = FindInterfaces(NetworkInterfaceType.WirelessAP);
                }

                return _apInterface;
            }
        }

        private Wireless80211Configuration _wifiConfiguration;
        private Wireless80211Configuration wifiConfiguration
        {
            get
            {
                if (_wifiConfiguration == null)
                {
                    _wifiConfiguration = Wireless80211Configuration.GetAllWireless80211Configurations()[wifiInterface.SpecificConfigId];
                }

                return _wifiConfiguration;
            }
        }

        private WirelessAPConfiguration _apConfiguration;
        private WirelessAPConfiguration apConfiguration
        {
            get
            {
                if (_apConfiguration == null)
                {
                    _apConfiguration = WirelessAPConfiguration.GetAllWirelessAPConfigurations()[apInterface.SpecificConfigId];
                }

                return _apConfiguration;
            }
        }

        private DhcpServer _dhcpServer;
        private readonly IConfigurationService _configService;
        private readonly ILog _logger;
        private AutoResetEvent _semaphore;
        private WiFiNetworkInfo[] _networks;

        public WiFiInterfaceManager(IConfigurationService configService, ILog logger)
        {
            _configService = configService;
            _logger = logger;
        }

        /// <inheritdoc />
        public bool IsWifiEnabled => (wifiConfiguration.Options & Wireless80211Configuration.ConfigurationOptions.Enable)
                                     == Wireless80211Configuration.ConfigurationOptions.Enable;

        /// <inheritdoc />
        public bool IsApEnabled => (apConfiguration.Options & WirelessAPConfiguration.ConfigurationOptions.Enable)
                                   == WirelessAPConfiguration.ConfigurationOptions.Enable;

        /// <inheritdoc />
        public string WifiIpAddress => wifiInterface.IPv4Address;
        
        /// <inheritdoc />
        public string ApIpAddress => apInterface.IPv4Address;

        /// <inheritdoc />
        public bool HasPendingChanges { get; private set; }

        /// <inheritdoc />
        public void EnableWiFi()
        {
            wifiConfiguration.Options = Wireless80211Configuration.ConfigurationOptions.Enable;
            wifiConfiguration.SaveConfiguration();
            _logger.Trace("WiFi enabled");
            HasPendingChanges = true;
        }

        /// <inheritdoc />
        public void DisableWiFi()
        {
            wifiConfiguration.Options = Wireless80211Configuration.ConfigurationOptions.Disable;
            wifiConfiguration.SaveConfiguration();
            _logger.Trace("WiFi disabled");
            HasPendingChanges = true;
        }

        /// <inheritdoc />
        public void EnableAP()
        {
            var config = GetConfig();
            apInterface.EnableStaticIPv4(config.IpAddress, config.SubnetMask, config.IpAddress);
            apConfiguration.Ssid = config.Ssid;
            if (String.IsNullOrEmpty(config.Password))
            {
                apConfiguration.Authentication = AuthenticationType.Open;
            }
            else
            {
                apConfiguration.Authentication = AuthenticationType.WPA2;
                apConfiguration.Password = config.Password;
            }

            apConfiguration.MaxConnections = config.MaxConnections;

            apConfiguration.Options = WirelessAPConfiguration.ConfigurationOptions.Enable |
                                       WirelessAPConfiguration.ConfigurationOptions.AutoStart;
            apConfiguration.SaveConfiguration();

            _logger.Trace("AP enabled");

            HasPendingChanges = true;
        }

        /// <inheritdoc />
        public void DisableAP()
        {
            apConfiguration.Options = WirelessAPConfiguration.ConfigurationOptions.Disable;
            apConfiguration.SaveConfiguration();

            _logger.Trace("AP enabled");

            HasPendingChanges = true;
        }

        /// <inheritdoc />
        public void StartDhcp()
        {
            var config = GetConfig();
            _dhcpServer ??= new DhcpServer { CaptivePortalUrl = $"http://{config.IpAddress}" };

            Invoker.Retry(() =>
            {
                var dhcpInitResult =
                    _dhcpServer.Start(IPAddress.Parse(config.IpAddress), IPAddress.Parse(config.SubnetMask));
                if (!dhcpInitResult)
                    throw new Exception("DHCP failed to start");

                _logger.Trace("DHCP started");
            }, 3);
        }

        /// <inheritdoc />
        public void StopDhcp()
        {
            _dhcpServer?.Stop();
            _logger.Trace($"DHCP stopped");
        }

        /// <inheritdoc />
        public WiFiNetworkInfo[] GetAvailableNetworks()
        {
            _logger.Trace("GetAvailableNetworks()");

            _semaphore ??= new AutoResetEvent(false);
            _semaphore.Reset();

            var wifi = WifiAdapter.FindAllAdapters()[0];
            wifi.AvailableNetworksChanged += WifiOnAvailableNetworksChanged;
            _networks = new WiFiNetworkInfo[0];

            wifi.ScanAsync();
            _logger.Trace("Scanning, waiting");

            _semaphore.WaitOne(60000, false);

            wifi.AvailableNetworksChanged -= WifiOnAvailableNetworksChanged;
            
            _logger.Trace($"done, networks found: {_networks.Length}");
            
            return _networks;
        }

        /// <inheritdoc />
        public void DisconnectWifi()
        {
            _logger.Trace("Wifi disconnecting");
            var adapters = WifiAdapter.FindAllAdapters();
            adapters[0].Disconnect();
            _logger.Trace($"{wifiInterface.IPv4Address}");

        }

        private void WifiOnAvailableNetworksChanged(WifiAdapter sender, object e)
        {
            var networkReport = sender.NetworkReport;

            _logger.Trace($"AvailableNetworksChanged, AvailableNetworks.Length: {networkReport.AvailableNetworks.Length}");

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

            _logger.Trace($"networks count {_networks.Length}");
            if (_networks.Length > 0)
            {
                _logger.Trace("semaphore set");
                _semaphore.Set();
            }
        }

        private NetworkInterface FindInterfaces(NetworkInterfaceType interfaceType)
        {
            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.NetworkInterfaceType == interfaceType)
                {
                    return networkInterface;
                }
            }

            throw new InvalidOperationException();
        }

        private WiFiAPConfig GetConfig()
        {
            return (WiFiAPConfig)_configService.GetConfigSection(WiFiAPConfig.SectionName, typeof(WiFiAPConfig));
        }
    }
}
