using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using VolMixerConsole.Logging;

namespace VolMixerConsole
{
    /// <summary>
    /// manages windows audio mixer values using a serial stream as input.
    /// See: https://github.com/shpeeler/volmixer_arduino
    /// </summary>
    public class VolMixer
    {
        readonly string basePortname = "Pin_{0}";
        
        SerialPort port;
        readonly ILog logger;
        readonly IDictionary<string, string> portMapping;
        readonly int maxRetries;

        IDictionary<string, int> processMapping;

        /// <summary>
        /// creates an instance of <see cref="VolMixer"/>
        /// </summary>
        /// <param name="portName">the ports name</param>
        /// <param name="baudRate">the ports baudrate</param>
        /// <param name="maxRetries">maximum amount of retries when opening the port</param>
        /// <param name="pinMapping">mapping from </param>
        /// <param name="logger">instance of <see cref="ILog"/></param>
        public VolMixer(string portName, int baudRate, int maxRetries, IDictionary<string, string> pinMapping, ILog logger)
        {
            this.port = new SerialPort
            {
                PortName = portName,
                BaudRate = baudRate
            };

            this.maxRetries = maxRetries;
            this.portMapping = pinMapping;
            this.logger = logger;

            this.processMapping = new Dictionary<string, int>();
            foreach (KeyValuePair<string, string> pair in pinMapping)
            {
                if (string.IsNullOrEmpty(pair.Value))
                {
                    this.logger.LogWarning("no value for key: {0}", pair.Key);
                    continue;
                }

                int processId;
                if (TryGetProcessIdByName(pair.Value, out processId) == false)
                {
                    this.logger.LogWarning("unable to find a process with the id: '{0}'", processId.ToString());
                    continue;
                }
                this.processMapping.Add(pair.Value, processId);
            }
        }

        /// <summary>
        /// runs the <see cref="VolMixer"/>
        /// </summary>
        public void Run()
        {
            if (port.IsOpen == false)
            {
                int tryCount = 1;
                while (port.IsOpen == false && tryCount <= maxRetries)
                {
                    try
                    {
                        this.logger.LogInfo("trying to open port: '{0}'", port.PortName);
                        port.Open();
                    }
                    catch (Exception e)
                    {
                        this.logger.LogWarning(string.Format("unable to open the port: '{0}' tried: '{1}' times.", port.PortName, tryCount++), e.Message);
                        Thread.Sleep(100);
                    }
                }
                if (port.IsOpen == false)
                {
                    this.logger.LogError("port: '{0}' could not be opened", port.PortName);
                    return;
                }
            }
            
            this.logger.LogInfo("port: '{0}' open, start reading", port.PortName);
            while (true)
            {
                string value = port.ReadLine();
                if (string.IsNullOrEmpty(value) == false)
                {
                    string[] values = value.Split(':');
                    if (values.Length != 2)
                    {
                        this.logger.LogWarning("received value's length after splitting is not 2. Value: '{0}'", value);
                        continue;
                    }

                    string application;
                    if (this.portMapping.TryGetValue(string.Format(basePortname, values[0]), out application) == false)
                    {
                        this.logger.LogError("unable to find config key for: '{0}'", values[0]);
                        continue;
                    }

                    if (string.IsNullOrEmpty(application))
                    {
                        this.logger.LogWarning("no value set for key: '{0}' in PinMapping-Config", values[0]);
                        continue;
                    }

                    int processId;
                    if (this.processMapping.TryGetValue(application, out processId) == false)
                    {
                        this.logger.LogWarning("unable to find process mapping for: '{0}'", values[0]);
                        continue;
                    }

                    float volume;
                    if (float.TryParse(values[1], out volume) == false)
                    {
                        this.logger.LogError("unable to parse value: '{0}' to float", values[1]);
                        continue;
                    }

                    try
                    {
                        // trying to find the process with given id
                        Process.GetProcessById(processId);
                    }
                    catch
                    {
                        // if unable to find the process, run another lookup and store value in dict
                        this.logger.LogWarning("process with id: {0} no longer exists. trying to find it again.", processId.ToString());

                        int newProcessId;
                        if (TryGetProcessIdByName(application, out newProcessId) == false)
                        {
                            this.logger.LogError("unable to find a process with the id: '{0}'", processId.ToString());
                            continue;
                        }
                        processId = newProcessId;
                        this.processMapping[application] = newProcessId;
                    }

                    VolumeMixer.SetApplicationVolume(processId, volume);
                }
            }
        }

        /// <summary>
        /// trys to find a process with the given name <paramref name="procName"/>
        /// specificially looks for processes which are present in the windows volume mixer
        /// </summary>
        /// <param name="procName">process name</param>
        /// <returns>process id</returns>
        private bool TryGetProcessIdByName(string procName, out int processId)
        {
            processId = -1;

            // go through all processed
            foreach (Process process in Process.GetProcesses())
            {
                // filter processes which are available in mixer
                if (VolumeMixer.GetApplicationVolume(process.Id) != null)
                {
                    // if processname equals the seached name
                    if (process.ProcessName.ToUpper() == procName.ToUpper())
                    {
                        processId = process.Id;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
