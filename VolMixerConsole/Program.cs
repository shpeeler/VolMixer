using System;
using System.Collections.Generic;
using System.Configuration;

namespace VolMixerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            string portname = ConfigurationManager.AppSettings["Portname"];

            int baudrate;
            if (int.TryParse(ConfigurationManager.AppSettings["Baudrate"], out baudrate) == false)
            {
                Console.WriteLine("error parsing: Baudrate from AppConfig");
                return;
            }

            int maxRetries;
            if (int.TryParse(ConfigurationManager.AppSettings["MaxRetries"], out maxRetries) == false)
            {
                Console.WriteLine("error parsing: MaxRetries from AppConfig");
                return;
            }


            VolMixer volMixer = new VolMixer(portname, baudrate, maxRetries, ReadPortMappingFromConfig());
            try
            {
                volMixer.Run();
            }
            catch (Exception e)
            {
                Console.WriteLine("error while running VolMixer: {0}", e.Message);
            }
        }

        static IDictionary<string, string> ReadPortMappingFromConfig()
        {
            IDictionary<string, string> portMapping = new Dictionary<string, string>();

            foreach(string key in ConfigurationManager.AppSettings)
            {
                if (key.StartsWith("Port_"))
                {
                    portMapping.Add(key, ConfigurationManager.AppSettings[key]);
                }
            }

            return portMapping;
        }
    }
}
