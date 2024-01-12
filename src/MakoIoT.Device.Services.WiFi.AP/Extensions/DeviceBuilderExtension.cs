using MakoIoT.Device.Services.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace MakoIoT.Device.Services.WiFi.AP.Extensions
{
    public static class DeviceBuilderExtension
    {
        public static IDeviceBuilder AddWiFiInterfaceManager(this IDeviceBuilder builder)
        {
            builder.Services.AddTransient(typeof(INetworkInterfaceManager), typeof(WiFiInterfaceManager));

            return builder;
        }
    }
}
