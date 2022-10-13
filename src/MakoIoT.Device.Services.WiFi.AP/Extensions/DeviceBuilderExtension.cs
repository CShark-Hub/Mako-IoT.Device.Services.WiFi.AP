using MakoIoT.Device.Services.DependencyInjection;
using MakoIoT.Device.Services.Interface;

namespace MakoIoT.Device.Services.WiFi.AP.Extensions
{
    public static class DeviceBuilderExtension
    {
        public static IDeviceBuilder AddWiFiInterfaceManager(this IDeviceBuilder builder)
        {
            DI.RegisterSingleton(typeof(INetworkInterfaceManager), typeof(WiFiInterfaceManager));

            return builder;
        }
    }
}
