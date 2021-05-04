using System.Collections.Generic;

namespace VolMixer.Components
{
    /// <summary>
    /// represents the configurations for <see cref="VolMixer"/>
    /// </summary>
    public class VolMixerConfig
    {
        /// <summary>
        /// com-portname
        /// </summary>
        public string Portname { get; set; }

        /// <summary>
        /// baudrate of serial connection
        /// </summary>
        public int Baudrate { get; set; }

        /// <summary>
        /// maximum com-connection-retries before terminating
        /// </summary>
        public int MaxRetries { get; set; }

        /// <summary>
        /// sound output device name
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// mapping from pin to application
        /// </summary>
        public IDictionary<string, string> PinMapping { get; set; }

        /// <summary>
        /// mapping from application to process-ids
        /// </summary>
        public IDictionary<string, IList<int>> ProcessMapping { get; set; }

        /// <summary>
        /// creates an instance of <see cref="VolMixerConfig"/>
        /// </summary>
        /// <param name="pPortname">com-portname</param>
        /// <param name="pBaudrate">baudrate of serial connection</param>
        /// <param name="pMaxRetries">maximum com-connection-retries before terminating</param>
        /// <param name="pDeviceName">sound output device name</param>
        /// <param name="pPinMapping">mapping from pin to application</param>
        /// <param name="pProcessmapping">mapping from application to process-ids</param>
        public VolMixerConfig(string pPortname, int pBaudrate, int pMaxRetries, string pDeviceName, IDictionary<string, string> pPinMapping, IDictionary<string, IList<int>> pProcessmapping)
        {
            this.Portname = pPortname;
            this.Baudrate = pBaudrate;
            this.MaxRetries = pMaxRetries;
            this.DeviceName = pDeviceName;
            this.PinMapping = pPinMapping;
            this.ProcessMapping = pProcessmapping;
        }
    }
}
