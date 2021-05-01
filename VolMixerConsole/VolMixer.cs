using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;

namespace VolMixerConsole
{
    public class VolMixer
    {
        SerialPort port;
        readonly int maxRetries;
        readonly IDictionary<string, string> portMapping;
        readonly string nircmdLocation;

        readonly string baseCmd = @"{0} setappvolume {1} {2}";
        readonly string basePortname = "Port_{0}";
        readonly string baseVolume = "0.{0}";

        public VolMixer(string portName, int baudRate, int maxRetries, IDictionary<string, string> portMapping, string nircmdLocation)
        {
            this.port = new SerialPort
            {
                PortName = portName,
                BaudRate = baudRate
            };

            this.maxRetries = maxRetries;
            this.portMapping = portMapping;
            this.nircmdLocation = nircmdLocation;
        }

        public void Run()
        {
            if (port.IsOpen == false)
            {
                int tryCount = 1;
                while (port.IsOpen == false && tryCount <= maxRetries)
                {
                    try
                    {
                        Console.WriteLine("trying to open port");
                        port.Open();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("error when trying to open the port. Try: {0} - Exception: {1}", tryCount++, e.Message);
                        Thread.Sleep(100);
                    }
                }
                if (port.IsOpen == false)
                {
                    Console.WriteLine("port could not be opened");
                    return;
                }
            }

            Console.WriteLine("port opened, start reading");
            while (true)
            {
                string value = port.ReadLine();
                if (string.IsNullOrEmpty(value) == false)
                {
                    string[] values = value.Split(':');
                    if(values.Length != 2)
                    {
                        Console.WriteLine("error: value length after split is not as expected. String: {0}", value);
                        continue;
                    }

                    string application;
                    if (this.portMapping.TryGetValue(string.Format(basePortname, values[0]), out application) == false)
                    {
                        Console.WriteLine("error: could not find config key for: {0}", values[0]);
                    }
                    string volume = string.Format(baseVolume, values[1]);

                    string cmd = string.Format(baseCmd, nircmdLocation, application, volume).Trim('\n', '\r');

                    System.Diagnostics.ProcessStartInfo proc = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe",
                        Arguments = cmd,
                        CreateNoWindow = true
                    };
                    
                    System.Diagnostics.Process.Start(proc);
                }
            }
        }
    }
}
