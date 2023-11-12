﻿using MakoIoT.Device.Services.Interface;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Mako_IoT.Device.Services.WiFi.AP.Test.Mocks
{
    internal class DeviceBuilderMock : IDeviceBuilder
    {
        public IServiceCollection Services { get; }

        public ConfigureDefaultsDelegate ConfigureDefaultsAction { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event DeviceStartingDelegate DeviceStarting;
        public event DeviceStoppedDelegate DeviceStopped;

        public DeviceBuilderMock()
        {
            Services = new ServiceCollection();
        }

        public IDeviceBuilder ConfigureDI(ConfigureDIDelegate configureDiAction)
        {
            throw new NotImplementedException();
        }

        public IDevice Build()
        {
            throw new NotImplementedException();
        }

        public IDeviceBuilder ConfigureDI(Action configureDiAction)
        {
            throw new NotImplementedException();
        }
    }
}
