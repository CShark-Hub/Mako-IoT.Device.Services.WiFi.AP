using nanoFramework.TestFramework;
using Microsoft.Extensions.DependencyInjection;
using Mako_IoT.Device.Services.WiFi.AP.Test.Mocks;
using MakoIoT.Device.Services.WiFi.AP.Extensions;
using MakoIoT.Device.Services.Interface;
using MakoIoT.Device.Services.WiFi.AP;

namespace Mako_IoT.Device.Services.WiFi.AP.Test.Extensions
{
    [TestClass]
    public class DeviceBuilderExtensionTests
    {
        [TestMethod]
        public void AddWiFiInterfaceManager_Should_RegisterServices()
        {
            var mockBuilder = new DeviceBuilderMock();
            mockBuilder.Services.AddSingleton(typeof(IConfigurationService), new ConfigurationServiceMock());
            mockBuilder.Services.AddSingleton(typeof(ILog), new LoggerMock());

            mockBuilder.AddWiFiInterfaceManager();

            var serviceProvider = mockBuilder.Services.BuildServiceProvider();
            var result = serviceProvider.GetService(typeof(INetworkInterfaceManager));
            Assert.IsNotNull(result);
        }
    }
}
