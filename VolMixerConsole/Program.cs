using System;
using System.Collections.Generic;
using System.Configuration;
using log4net;

namespace VolMixerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            ILog log = LogManager.GetLogger(nameof(VolMixer));
            if (log == null)
            {
                throw new Exception("unable to initialize logger");
            }

            string portname = ConfigurationManager.AppSettings["Portname"];

            int baudrate;
            if (int.TryParse(ConfigurationManager.AppSettings["Baudrate"], out baudrate) == false)
            {
                log.Error("unable to parse config key: 'Baudrate' to int");
                return;
            }

            int maxRetries;
            if (int.TryParse(ConfigurationManager.AppSettings["MaxRetries"], out maxRetries) == false)
            {
                log.Error("unable to parse config key: 'MaxRetries' to int");
                return;
            }

            VolMixer volMixer = new VolMixer(portname, baudrate, maxRetries, ReadPortMappingFromConfig(), log);
            try
            {
                volMixer.Run();
            }
            catch (Exception e)
            {
                log.Error(string.Format("error while running {0}", nameof(VolMixer)), e);
            }
        }

        static IDictionary<string, string> ReadPortMappingFromConfig()
        {
            IDictionary<string, string> portMapping = new Dictionary<string, string>();

            foreach(string key in ConfigurationManager.AppSettings)
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
