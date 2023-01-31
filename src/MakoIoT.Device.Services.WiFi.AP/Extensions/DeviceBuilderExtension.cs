using MakoIoT.Device.Services.Interface;
using nanoFramework.DependencyInjection;

namespace MakoIoT.Device.Services.WiFi.AP.Extensions
{
    public static class DeviceBuilderExtension
    {
        public static IDeviceBuilder AddWiFiInterfaceManager(this IDeviceBuilder builder)
        {
            builder.Services.AddSingleton(typeof(INetworkInterfaceManager), typeof(WiFiInterfaceManager));

            return builder;
        }
    }
}
