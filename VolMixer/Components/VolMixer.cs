using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using log4net;

namespace VolMixer.Components
{
    /// <summary>
    /// manages windows audio mixer values using a serial stream as input.
    /// See: https://github.com/shpeeler/volmixer_arduino
    /// </summary>
    public class VolMixer
    {
        VolMixerConfig volMixerConfig;
        readonly SerialPort port;
        readonly ILog log;

        /// <summary>
        /// creates an instance of <see cref="VolMixer"/>
        /// </summary>
        /// <param name="portName">the ports name</param>
        /// <param name="baudRate">the ports baudrate</param>
        /// <param name="maxRetries">maximum amount of retries when opening the port</param>
        /// <param name="pinMapping">mapping from </param>
        /// <param name="pLog">instance of <see cref="ILog"/></param>
        public VolMixer(ILog pLog, VolMixerConfig pVolMixerConfig)
        {
            this.port = new SerialPort
            {
                PortName = pVolMixerConfig.Portname,
                BaudRate = pVolMixerConfig.Baudrate
            };

            this.volMixerConfig = pVolMixerConfig;
            this.log = pLog;
        }

        /// <summary>
        /// runs the <see cref="VolMixer"/>
        /// </summary>
        public void Run()
        {
            this.log.Debug(string.Format(VolMixerHelper.BASE_METHOD_ENTERING_LOG, nameof(VolMixer), nameof(Run)));

            if (TryOpenPort() == false)
            {
                return;
            }

            while (true)
            {
                string value = port.ReadLine();
                if (string.IsNullOrEmpty(value) == false)
                {
                    if(TryGetInfoFromSerial(value, out string application, out IList<int> processIds, out float volume) == false)
                    {
                        continue;
                    }

                    if (processIds.Count == 0)
                    {
                        IList<int> newProcessIds;
                        if (VolMixerHelper.TryGetProcessIdsByApplicationName(this.log, application, this.volMixerConfig.DeviceName, out newProcessIds) == false)
                        {
                            this.log.Error(string.Format("unable to find a with the name: '{0}'", application));
                            continue;
                        }
                        processIds = newProcessIds;
                        this.volMixerConfig.ProcessMapping[application] = newProcessIds;
                    }

                    // check if all processes exist
                    foreach (int processId in processIds)
                    {
                        try
                        {
                            //
                            // trying to find the process with given id
                            //

                            Process.GetProcessById(processId);
                        }
                        catch
                        {
                            this.log.Warn(string.Format("process with id: {0} for application: {1} no longer exists. trying to find it again.", processId, application));
                            
                            //
                            // if unable to find the process, run another lookup and store value in dict
                            //

                            IList<int> newProcessIds;
                            if (VolMixerHelper.TryGetProcessIdsByApplicationName(this.log, application, this.volMixerConfig.DeviceName, out newProcessIds) == false)
                            {
                                this.log.Error(string.Format("unable to find a with the name: '{0}'", application));
                                continue;
                            }
                            processIds = newProcessIds;
                            this.volMixerConfig.ProcessMapping[application] = newProcessIds;
                        }
                    }

                    // change volume for each process
                    foreach(int processId in processIds)
                    {
                        if (VolMixerHelper.TrySetApplicationVolume(processId, volMixerConfig.DeviceName, volume) == false)
                        {
                            this.log.Error(string.Format("unable to change audio\n.device: {0}\napplication {1}\nprocess id: {2}", volMixerConfig.DeviceName, application, processId));
                        }
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
            this.log.Debug(string.Format(VolMixerHelper.BASE_METHOD_ENTERING_LOG, nameof(VolMixer), nameof(TryOpenPort)));

            if (port.IsOpen == false)
            {
                int tryCount = 1;
                while (port.IsOpen == false && tryCount <= this.volMixerConfig.MaxRetries)
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

            this.log.Debug(string.Format(VolMixerHelper.BASE_METHOD_LEAVING_LOG, nameof(VolMixer), nameof(TryOpenPort)));
            return true;
        }

        /// <summary>
        /// trys to gather all needed informatino from the serial input
        /// </summary>
        /// <param name="pValue">serial input</param>
        /// <param name="oApplication">target application name</param>
        /// <param name="oProcessId">target process-id</param>
        /// <param name="oVolume">target volume</param>
        /// <returns>true if successful, false if not</returns>
        private bool TryGetInfoFromSerial(string pValue, out string oApplication, out IList<int> oProcessIds, out float oVolume)
        {
            this.log.Debug(string.Format(VolMixerHelper.BASE_METHOD_ENTERING_LOG, nameof(VolMixer), nameof(TryGetInfoFromSerial)));
            
            oApplication = null;
            oProcessIds = new List<int>();
            oVolume = 0;

            string[] values = pValue.Split(':');
            if (values.Length != 2)
            {
                this.log.Warn(string.Format("received value's length after splitting is not 2. Value: '{0}'", pValue));
                return false;
            }

            if (this.volMixerConfig.PinMapping.TryGetValue(string.Format(VolMixerHelper.BASE_PIN_NAME, values[0]), out oApplication) == false)
            {
                this.log.Error(string.Format("unable to find config key for: '{0}'", values[0]));
                return false;
            }

            if (string.IsNullOrEmpty(oApplication))
            {
                this.log.Warn(string.Format("no value set for key: '{0}' in PinMapping-Config", values[0]));
                return false;
            }

            if (this.volMixerConfig.ProcessMapping.TryGetValue(oApplication, out oProcessIds) == false)
            {
                this.log.Warn(string.Format("unable to find process mapping for: '{0}'", values[0]));
                return false;
            }

            if (float.TryParse(values[1], out oVolume) == false)
            {
                this.log.Error(string.Format("unable to parse value: '{0}' to float", values[1]));
                return false;
            }

            this.log.Debug(string.Format(VolMixerHelper.BASE_METHOD_LEAVING_LOG, nameof(VolMixer), nameof(TryGetInfoFromSerial)));
            return true;
        }
    }
}
