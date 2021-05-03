using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using log4net;

namespace VolMixerConsole
{
    /// <summary>
    /// manages windows audio mixer values using a serial stream as input.
    /// See: https://github.com/shpeeler/volmixer_arduino
    /// </summary>
    public class VolMixer
    {
        readonly string basePortname = "Pin_{0}";
        readonly string baseMethodEnterLog = "{0}:{1} - entering";
        readonly string baseMethodLeavingLog = "{0}:{1} - leaving";
        
        SerialPort port;
        readonly ILog log;
        readonly IDictionary<string, string> portMapping;
        readonly int maxRetries;
        readonly VolMixerHelper volMixerHelper;
        readonly string deviceName;

        IDictionary<string, int> processMapping;

        /// <summary>
        /// creates an instance of <see cref="VolMixer"/>
        /// </summary>
        /// <param name="portName">the ports name</param>
        /// <param name="baudRate">the ports baudrate</param>
        /// <param name="maxRetries">maximum amount of retries when opening the port</param>
        /// <param name="pinMapping">mapping from </param>
        /// <param name="log">instance of <see cref="ILog"/></param>
        public VolMixer(string portName, int baudRate, int maxRetries, IDictionary<string, string> pinMapping, ILog log, string deviceName)
        {
            this.port = new SerialPort
            {
                PortName = portName,
                BaudRate = baudRate
            };

            this.maxRetries = maxRetries;
            this.portMapping = pinMapping;
            this.log = log;
            this.deviceName = deviceName;

            volMixerHelper = new VolMixerHelper(log);

            this.processMapping = CreateProcessMapping(pinMapping);
        }

        /// <summary>
        /// runs the <see cref="VolMixer"/>
        /// </summary>
        public void Run()
        {
            this.log.Debug(string.Format(baseMethodEnterLog, nameof(VolMixer), nameof(Run)));

            if (TryOpenPort() == false)
            {
                return;
            }

            while (true) // figure out what to do with this
            {
                string value = port.ReadLine();
                if (string.IsNullOrEmpty(value) == false)
                {
                    if(TryGetInfoFromSerial(value, out string application, out int processId, out float volume) == false)
                    {
                        continue;
                    }

                    try
                    {
                        // trying to find the process with given id
                        Process.GetProcessById(processId);
                    }
                    catch
                    {
                        //
                        // if unable to find the process, run another lookup and store value in dict
                        //

                        this.log.Warn(string.Format("process with id: {0} no longer exists. trying to find it again.", processId));

                        int newProcessId;
                        if (TryGetProcessIdByName(application, out newProcessId) == false)
                        {
                            this.log.Error(string.Format("unable to find a with the name: '{0}'", application));
                            continue;
                        }
                        processId = newProcessId;
                        this.processMapping[application] = newProcessId;
                    }

                    if(this.volMixerHelper.TrySetApplicationVolume(processId, this.deviceName, volume))
                    {

                    }
                }
            }
        }

        /// <summary>
        /// trys to open the configured port
        /// </summary>
        /// <returns>true if successful, false if not</returns>
        private bool TryOpenPort()
        {
            this.log.Debug(string.Format(baseMethodEnterLog, nameof(VolMixer), nameof(TryOpenPort)));

            if (port.IsOpen == false)
            {
                int tryCount = 1;
                while (port.IsOpen == false && tryCount <= maxRetries)
                {
                    try
                    {
                        this.log.Info(string.Format("trying to open port: '{0}'", port.PortName));
                        port.Open();
                    }
                    catch (Exception e)
                    {
                        this.log.Warn(string.Format("unable to open the port: '{0}' tried: '{1}' times.", port.PortName, tryCount++), e);
                        Thread.Sleep(100);
                    }
                }
                if (port.IsOpen == false)
                {
                    this.log.Error(string.Format("port: '{0}' could not be opened", port.PortName));
                    return false;
                }
            }
            this.log.Info(string.Format("port: '{0}' open, start reading", port.PortName));

            this.log.Debug(string.Format(baseMethodLeavingLog, nameof(VolMixer), nameof(TryOpenPort)));
            return true;
        }

        /// <summary>
        /// trys to gather all needed informatino from the serial input
        /// </summary>
        /// <param name="value">serial input</param>
        /// <param name="application">target application name</param>
        /// <param name="processId">target process-id</param>
        /// <param name="volume">target volume</param>
        /// <returns>true if successful, false if not</returns>
        private bool TryGetInfoFromSerial(string value, out string application, out int processId, out float volume)
        {
            this.log.Debug(string.Format(baseMethodEnterLog, nameof(VolMixer), nameof(TryGetInfoFromSerial)));
            
            application = null;
            processId = -1;
            volume = 0;

            string[] values = value.Split(':');
            if (values.Length != 2)
            {
                this.log.Warn(string.Format("received value's length after splitting is not 2. Value: '{0}'", value));
                return false;
            }

            if (this.portMapping.TryGetValue(string.Format(basePortname, values[0]), out application) == false)
            {
                this.log.Error(string.Format("unable to find config key for: '{0}'", values[0]));
                return false;
            }

            if (string.IsNullOrEmpty(application))
            {
                this.log.Warn(string.Format("no value set for key: '{0}' in PinMapping-Config", values[0]));
                return false;
            }

            if (this.processMapping.TryGetValue(application, out processId) == false)
            {
                this.log.Warn(string.Format("unable to find process mapping for: '{0}'", values[0]));
                return false;
            }

            if (float.TryParse(values[1], out volume) == false)
            {
                this.log.Error(string.Format("unable to parse value: '{0}' to float", values[1]));
                return false;
            }

            this.log.Debug(string.Format(baseMethodLeavingLog, nameof(VolMixer), nameof(TryGetInfoFromSerial)));
            return true;
        }

        /// <summary>
        /// trys to find a process with the given name <paramref name="procName"/>
        /// specificially looks for processes which are present in the windows volume mixer
        /// </summary>
        /// <param name="procName">process name</param>
        /// <returns>process id</returns>
        private bool TryGetProcessIdByName(string procName, out int processId)
        {
            this.log.Debug(string.Format(baseMethodEnterLog, nameof(VolMixer), nameof(TryGetInfoFromSerial)));

            processId = -1;

            // go through all processed
            foreach (Process process in Process.GetProcesses())
            {
                // filter processes which are available in mixer
                if (this.volMixerHelper.TryGetApplicationVolume(process.Id, this.deviceName, out float? oVolume))
                {
                    // if processname equals the seached name
                    if (process.ProcessName.ToUpper() == procName.ToUpper())
                    {
                        processId = process.Id;

                        this.log.Debug(string.Format(baseMethodLeavingLog, nameof(VolMixer), nameof(TryGetInfoFromSerial)));
                        return true;
                    }
                }
            }

            this.log.Debug(string.Format(baseMethodLeavingLog, nameof(VolMixer), nameof(TryGetInfoFromSerial)));
            return false;
        }

        /// <summary>
        /// creates a mapping from application-name to process id based on the given pin-mapping
        /// </summary>
        /// <param name="pinMapping">pin-mapping from app-config</param>
        /// <returns><see cref="IDictionary{TKey, TValue}"/> application-name to process-id</returns>
        private IDictionary<string, int> CreateProcessMapping(IDictionary<string, string> pinMapping)
        {
            this.log.Debug(string.Format(baseMethodEnterLog, nameof(VolMixer), nameof(TryGetInfoFromSerial)));

            IDictionary<string, int> processMapping = new Dictionary<string, int>();

            // try to get process for each application in given pinMapping
            foreach (KeyValuePair<string, string> pair in pinMapping)
            {
                if (string.IsNullOrEmpty(pair.Value))
                {
                    this.log.Warn(string.Format("no value for key: {0}", pair.Key));
                    continue;
                }

                int processId;
                if (TryGetProcessIdByName(pair.Value, out processId) == false)
                {
                    this.log.Warn(string.Format("unable to find a process with the name: '{0}'", pair.Value));
                }

                processMapping.Add(pair.Value, processId);
            }

            this.log.Debug(string.Format(baseMethodLeavingLog, nameof(VolMixer), nameof(TryGetInfoFromSerial)));
            return processMapping;
        }
    }
}
