using System;
using System.Collections.Generic;
using System.Configuration;
using VolMixer.Components;
using log4net;


namespace VolMixer.Console
{
    /// <summary>
    /// Console implementation for <see cref="VolMixer.Components.VolMixer"/>
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            ILog log = LogManager.GetLogger(nameof(Components.VolMixer));
            if (log == null)
            {
                throw new Exception("unable to initialize logger - terminating");
            }

            string portname = ConfigurationManager.AppSettings["Portname"];
            if (string.IsNullOrEmpty(portname))
            {
                log.Error("Portname value in app config is null - terminating");
                return;
            }

            string deviceName = ConfigurationManager.AppSettings["DeviceName"];
            if (string.IsNullOrEmpty(deviceName))
            {
                log.Error("DeviceName value in app config is null - terminating");
            }

            int baudrate;
            if (int.TryParse(ConfigurationManager.AppSettings["Baudrate"], out baudrate) == false)
            {
                log.Error("unable to parse config key: 'Baudrate' to int - terminating");
                return;
            }

            int maxRetries;
            if (int.TryParse(ConfigurationManager.AppSettings["MaxRetries"], out maxRetries) == false)
            {
                log.Error("unable to parse config key: 'MaxRetries' to int - terminating");
                return;
            }

            IDictionary<string, string> pinMapping = ReadPortMappingFromConfig();
            IDictionary<string, IList<int>> processMapping;
            if (VolMixerHelper.TryCreateProcessMapping(log, pinMapping, deviceName, out processMapping) == false)
            {
                log.Error("unable to initialize process mapping - terminating");
                return;
            }

            VolMixerConfig volMixerConfig = new VolMixerConfig(portname, baudrate, maxRetries, deviceName, pinMapping, processMapping);
            Components.VolMixer volMixer = new Components.VolMixer(log, volMixerConfig);
            try
            {
                volMixer.Run();
            }
            catch (Exception e)
            {
                log.Error(string.Format("error while running {0}", nameof(Components.VolMixer)), e);
            }
        }

        /// <summary>
        /// creates a port-mapping from the current app data configuration
        /// </summary>
        /// <returns><see cref="IDictionary{TKey, TValue}"/> pin-name to application</returns>
        private static IDictionary<string, string> ReadPortMappingFromConfig()
        {
            IDictionary<string, string> portMapping = new Dictionary<string, string>();

            foreach (string key in ConfigurationManager.AppSettings)
            {
                if (key.StartsWith("Pin_"))
                {
                    portMapping.Add(key, ConfigurationManager.AppSettings[key]);
                }
            }

            return portMapping;
        }
    }
}
