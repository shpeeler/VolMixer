using System;
using System.Collections.Generic;
using System.Configuration;
using VolMixerConsole.Logging;

namespace VolMixerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            ILog logger = InitLogger();
            if (logger == null)
            {
                throw new Exception("unable to initialize logger");
            }

            string portname = ConfigurationManager.AppSettings["Portname"];

            int baudrate;
            if (int.TryParse(ConfigurationManager.AppSettings["Baudrate"], out baudrate) == false)
            {
                logger.LogError("unable to parse config key: 'Baudrate' to int");
                return;
            }

            int maxRetries;
            if (int.TryParse(ConfigurationManager.AppSettings["MaxRetries"], out maxRetries) == false)
            {
                logger.LogError("unable to parse config key: 'MaxRetries' to int");
                return;
            }

            VolMixer volMixer = new VolMixer(portname, baudrate, maxRetries, ReadPortMappingFromConfig(), logger);
            try
            {
                volMixer.Run();
            }
            catch (Exception e)
            {
                logger.LogError(string.Format("error while running {0}", nameof(VolMixer)), e);
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

        static ILog InitLogger()
        {
            string logFilename = ConfigurationManager.AppSettings["LogFilename"];
            string logFilepath = ConfigurationManager.AppSettings["LogFilepath"];

            int logLevel;
            if (int.TryParse(ConfigurationManager.AppSettings["LogLevel"], out logLevel) == false)
            {
                throw new Exception("unable to parse config key: 'LogLevel' to int");
            }

            return new Logger(logFilename, logFilepath, (LogLevel)logLevel);
        }
    }
}
