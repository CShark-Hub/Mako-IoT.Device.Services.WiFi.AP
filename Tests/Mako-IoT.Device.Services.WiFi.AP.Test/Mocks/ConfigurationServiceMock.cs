using MakoIoT.Device.Services.Interface;
using MakoIoT.Device.Services.WiFi.AP.Configuration;
using System;

namespace Mako_IoT.Device.Services.WiFi.AP.Test.Mocks
{
    internal class ConfigurationServiceMock : IConfigurationService
    {
        public event EventHandler ConfigurationUpdated;

        public bool ClearAll()
        {
            throw new NotImplementedException();
        }

        public object GetConfigSection(string sectionName, Type objectType)
        {
            if (sectionName == WiFiAPConfig.SectionName)
            {
                return new WiFiAPConfig();
            }

            throw new NotImplementedException();
        }

        public string[] GetSections()
        {
            throw new NotImplementedException();
        }

        public string LoadConfigSection(string sectionName)
        {
            throw new NotImplementedException();
        }

        public void UpdateConfigSection(string sectionName, object section)
        {
            throw new NotImplementedException();
        }

        public bool UpdateConfigSectionString(string sectionName, string sectionString)
        {
            throw new NotImplementedException();
        }

        public bool UpdateConfigSectionString(string sectionName, string sectionString, Type objectType)
        {
            throw new NotImplementedException();
        }

        public void WriteDefault(string sectionName, object section, bool overwrite = false)
        {
            throw new NotImplementedException();
        }
    }
}
