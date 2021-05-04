using System;
using System.Threading;
using System.ServiceProcess;
using System.Configuration;
using System.Collections.Generic;
using VolMixer.Components;
using log4net;

namespace VolMixer.Service
{
    public partial class VolMixerService : ServiceBase
    {
        Thread workerThread;
        public ILog log;

        public VolMixerService()
        {
            InitializeComponent();

            log = LogManager.GetLogger(nameof(VolMixerService));
            if (log == null)
            {
                throw new Exception("unable to initialize logger");
            }
        }

        protected override void OnStart(string[] args)
        {
            this.log.Debug(string.Format(VolMixerHelper.BASE_METHOD_ENTERING_LOG, nameof(VolMixerService), nameof(OnStart)));

            string portname = ConfigurationManager.AppSettings["Portname"];
            if (string.IsNullOrEmpty(portname))
            {
                log.Error("Portname value in app config is null");
                return;
            }

            string deviceName = ConfigurationManager.AppSettings["DeviceName"];
            if (string.IsNullOrEmpty(deviceName))
            {
                log.Error("DeviceName value in app config is null");
            }

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

            IDictionary<string, string> pinMapping = ReadPortMappingFromConfig();
            IDictionary<string, IList<int>> processMapping;
            if (VolMixerHelper.TryCreateProcessMapping(log, pinMapping, deviceName, out processMapping) == false)
            {
                log.Error("unable to initialize process mapping - terminating");
                return;
            }

            VolMixerConfig volMixerConfig = new VolMixerConfig(portname, baudrate, maxRetries, deviceName, pinMapping, processMapping);

            Components.VolMixer volMixer = new Components.VolMixer(log, volMixerConfig);
            workerThread = new Thread(volMixer.Run);

            this.log.Info(string.Format("starting service: {0}", nameof(VolMixerService)));
            workerThread.Start();

            this.log.Debug(string.Format(VolMixerHelper.BASE_METHOD_LEAVING_LOG, nameof(VolMixerService), nameof(OnStart)));
        }

        protected override void OnStop()
        {
            this.log.Debug(string.Format(VolMixerHelper.BASE_METHOD_ENTERING_LOG, nameof(VolMixerService), nameof(OnStart)));

            if (workerThread.IsAlive)
            {
                this.log.Info(string.Format("stopping service: {0}", nameof(VolMixerService)));
                workerThread.Abort();
            }

            this.log.Debug(string.Format(VolMixerHelper.BASE_METHOD_LEAVING_LOG, nameof(VolMixerService), nameof(OnStart)));
        }

        /// <summary>
        /// reads all config keys starting with "Pin_" and maps them into a dictionary
        /// </summary>
        /// <returns><see cref="IDictionary{TKey, TValue}"/> pin to application</returns>
        private IDictionary<string, string> ReadPortMappingFromConfig()
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
