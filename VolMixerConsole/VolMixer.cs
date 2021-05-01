using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;

namespace VolMixerConsole
{
    public class VolMixer
    {
        SerialPort port;
        readonly int maxRetries;
        readonly IDictionary<string, string> portMapping;
        IDictionary<string, int> processMapping;

        readonly string baseArg = @"setappvolume {0} {1}";
        readonly string basePortname = "Port_{0}";
        readonly string baseVolume = "0.{0}";

        public VolMixer(string portName, int baudRate, int maxRetries, IDictionary<string, string> portMapping)
        {
            this.port = new SerialPort
            {
                PortName = portName,
                BaudRate = baudRate
            };

            this.maxRetries = maxRetries;
            this.portMapping = portMapping;

            this.processMapping = new Dictionary<string, int>();
            foreach (KeyValuePair<string, string> pair in portMapping)
            {
                if (string.IsNullOrEmpty(pair.Value))
                {
                    Console.WriteLine("no value for key: {0}", pair.Key);
                    continue;
                }

                int processId = GetProcessIdByName(pair.Value);
                if (processId == -1)
                {
                    Console.WriteLine("error: invalid process id: {0}", processId);
                    return;
                }
                this.processMapping.Add(pair.Value, processId);
            }
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
                    if (values.Length != 2)
                    {
                        Console.WriteLine("error: value length after split is not as expected. String: {0}", value);
                        continue;
                    }

                    string application;
                    if (this.portMapping.TryGetValue(string.Format(basePortname, values[0]), out application) == false)
                    {
                        Console.WriteLine("error: could not find config key for: {0}", values[0]);
                        continue;
                    }

                    float volume;
                    if (float.TryParse(values[1], out volume) == false)
                    {
                        Console.WriteLine("error: could not parse value: {0} to float", values[1]);
                        continue;
                    }

                    // find proc by id, if not avail then reload and change in dict
                    int processId = this.processMapping[application];
                    try
                    {
                        Process.GetProcessById(processId);
                    }
                    catch
                    {
                        Console.WriteLine("process with id: {0} no longer exists. trying to find it again.", processId);
                        int newProcessId = GetProcessIdByName(application); // TODO: move to method
                        if (newProcessId == -1)
                        {
                            Console.WriteLine("error: invalid process id: {0}", processId);
                            continue;
                        }

                        processId = newProcessId;
                        this.processMapping[application] = newProcessId;
                    }

                    VolumeMixer.SetApplicationVolume(processId, volume);
                }
            }
        }

        private int GetProcessIdByName(string procName)
        {
            int retVal = -1;

            // go through all processed
            foreach (Process process in Process.GetProcesses())
            {
                // filter processes which are available in mixer
                if (VolumeMixer.GetApplicationVolume(process.Id) != null)
                {
                    // if processname equals the seached name
                    if (process.ProcessName.ToUpper() == procName.ToUpper())
                    {
                        return process.Id;
                    }
                }
            }

            return retVal;
        }
    }
}
