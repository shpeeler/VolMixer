using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using log4net;
using VolMixerConsole.CoreAudio;
using VolMixerConsole.CoreAudio.Properties;
using VolMixerConsole.CoreAudio.Enums;
using VolMixerConsole.CoreAudio.Interfaces;

namespace VolMixerConsole.Components
{
    /// <summary>
    /// helper class for <see cref="VolMixer"/>
    /// </summary>
    public static class VolMixerHelper
    {
        /// <summary>
        /// base string for entering method logging
        /// </summary>
        public const string BASE_METHOD_ENTERING_LOG = "{0}:{1} - entering";

        /// <summary>
        /// base string for leaving method logging
        /// </summary>
        public const string BASE_METHOD_LEAVING_LOG = "{0}:{1} - leaving";

        /// <summary>
        /// base string for pin-names
        /// </summary>
        public const string BASE_PIN_NAME = "Pin_{0}";

        /// <summary>
        /// tries to set the application volume for volume for the application with the process-id <paramref name="pProcessId"/>
        /// </summary>
        /// <param name="pProcessId">process-id of target application</param>
        /// <param name="pDeviceName">sound output device name</param>
        /// <param name="pVolumeLevel">target volume level</param>
        /// <returns>true if succesful, false if not</returns>
        public static bool TrySetApplicationVolume(int pProcessId, string pDeviceName, float pVolumeLevel)
        {
            ISimpleAudioVolume simpleVolumeAudio;
            if (TryGetISimpleAudioVolumeByProcessId(pProcessId, pDeviceName, out simpleVolumeAudio) == false)
            {
                return false;
            }

            Guid guid = Guid.Empty;
            simpleVolumeAudio.SetMasterVolume(pVolumeLevel / 100, ref guid);

            Marshal.ReleaseComObject(simpleVolumeAudio);
            return true;
        }

        /// <summary>
        /// tries to get all process-ids for a sound-output-device
        /// </summary>
        /// <param name="pDeviceName">sound output device name</param>
        /// <param name="oProcessIds">all processes as <see cref="IList{T}"/></param>
        /// <returns>true if succesful, false if not</returns>
        public static bool TryGetProcessesForDevice(string pDeviceName, out IList<int> oProcessIds)
        {
            oProcessIds = new List<int>();

            IMMDevice targetDevice = GetIMMDeviceByName(pDeviceName);
            if (targetDevice == null)
            {
                return false;
            }

            Guid IID_IAudioSessionManager2 = typeof(IAudioSessionManager2).GUID;
            object o;
            targetDevice.Activate(ref IID_IAudioSessionManager2, 0, IntPtr.Zero, out o);
            IAudioSessionManager2 mgr = (IAudioSessionManager2)o;

            IAudioSessionEnumerator sessionEnumerator;
            mgr.GetSessionEnumerator(out sessionEnumerator);
            int count;
            sessionEnumerator.GetCount(out count);

            for (int i = 0; i < count; i++)
            {
                IAudioSessionControl2 ctl;
                sessionEnumerator.GetSession(i, out ctl);
                int cpid;
                ctl.GetProcessId(out cpid);

                oProcessIds.Add(cpid);

                Marshal.ReleaseComObject(ctl);
            }

            Marshal.ReleaseComObject(sessionEnumerator);
            Marshal.ReleaseComObject(mgr);
            Marshal.ReleaseComObject(targetDevice);

            return true;
        }

        /// <summary>
        /// tries to get all process-ids for an application
        /// </summary>
        /// <param name="pLog">instance of <see cref="ILog"/></param>
        /// <param name="pApplicationName">application name</param>
        /// <param name="pDeviceName">sound output device name</param>
        /// <param name="oProcessIds">all process-ids as <see cref="IList{T}"/></param>
        /// <returns>true if succesful, false if not</returns>
        public static bool TryGetProcessIdsByApplicationName(ILog pLog, string pApplicationName, string pDeviceName, out IList<int> oProcessIds)
        {
            pLog.Debug(string.Format(BASE_METHOD_ENTERING_LOG, nameof(VolMixerHelper), nameof(TryGetProcessIdsByApplicationName)));
            oProcessIds = new List<int>();

            IList<int> processIds;
            if (TryGetProcessesForDevice(pDeviceName, out processIds) == false)
            {
                pLog.Error(string.Format("unable to fetch processes for device: {0}", pDeviceName));
                pLog.Debug(string.Format(BASE_METHOD_LEAVING_LOG, nameof(VolMixerHelper), nameof(TryGetProcessIdsByApplicationName)));
                return false;
            }

            foreach (int eachProcessId in processIds)
            {
                Process resultProcess = null;
                try
                {
                    resultProcess = Process.GetProcessById(eachProcessId);
                }
                catch (Exception e)
                {
                    pLog.Error(string.Format("error while trying to get process: {0}", eachProcessId), e);
                }

                string resultProcessName = resultProcess.ProcessName;
                if (pApplicationName == resultProcessName)
                {
                    oProcessIds.Add(eachProcessId);
                }
            }

            pLog.Debug(string.Format(BASE_METHOD_LEAVING_LOG, nameof(VolMixerHelper), nameof(TryGetProcessIdsByApplicationName)));
            return true;
        }

        /// <summary>
        /// tries to create a process-mapping dictionary
        /// </summary>
        /// <param name="pLog">instance of <see cref="ILog"/></param>
        /// <param name="pPinMapping">pin-mapping from app config</param>
        /// <param name="pDeviceName">sound output device name</param>
        /// <param name="oProcessMapping"><see cref="IDictionary{TKey, TValue}"/> application-name to process-ids</param>
        /// <returns>true if succesful, false if not</returns>
        public static bool TryCreateProcessMapping(ILog pLog, IDictionary<string, string> pPinMapping, string pDeviceName, out IDictionary<string, IList<int>> oProcessMapping)
        {
            pLog.Debug(string.Format(BASE_METHOD_ENTERING_LOG, nameof(VolMixerHelper), nameof(TryCreateProcessMapping)));
            oProcessMapping = new Dictionary<string, IList<int>>();

            IList<int> processIds;
            if (TryGetProcessesForDevice(pDeviceName, out processIds) == false)
            {
                pLog.Error(string.Format("unable to fetch processes for device: {0}", pDeviceName));
                return false;
            }

            foreach (KeyValuePair<string, string> eachPair in pPinMapping)
            {
                if (string.IsNullOrEmpty(eachPair.Value))
                {
                    pLog.Warn(string.Format("no value for key: {0}", eachPair.Key));
                    continue;
                }

                IList<int> resultProcessIds = new List<int>();
                foreach (int eachProcessId in processIds)
                {
                    Process resultProcess = null;
                    try
                    {
                        resultProcess = Process.GetProcessById(eachProcessId);
                    }
                    catch (Exception e)
                    {
                        pLog.Error(string.Format("error while trying to get process: {0}", eachProcessId), e);
                    }

                    string resultProcessName = resultProcess.ProcessName;
                    if (eachPair.Value == resultProcessName)
                    {
                        resultProcessIds.Add(eachProcessId);
                    }
                }

                if (resultProcessIds.Count == 0)
                {
                    pLog.Warn(string.Format("no processes found for application: {0}", eachPair.Key));
                }

                oProcessMapping.Add(eachPair.Value, resultProcessIds);
            }

            pLog.Debug(string.Format(BASE_METHOD_LEAVING_LOG, nameof(VolMixerHelper), nameof(TryCreateProcessMapping)));
            return true;
        }

        /// <summary>
        /// tries to get an IMMDevice by its PKEY_DEVICE_FRIENDLY_NAME
        /// </summary>
        /// <param name="deviceFriendlyName">PKEY_DEVICE_FRIENDLY_NAME of sound output device</param>
        /// <returns>true if succesful, false if not</returns>
        private static IMMDevice GetIMMDeviceByName(string deviceFriendlyName)
        {
            IMMDeviceEnumerator deviceEnumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());

            IMMDeviceCollection deviceCollection;
            deviceEnumerator.EnumAudioEndpoints(EDataFlow.eRender, EDeviceState.Active, out deviceCollection);

            IMMDevice targetDevice = null;

            uint deviceCount;
            deviceCollection.GetCount(out deviceCount);
            for (uint i = 0; i < deviceCount; i++)
            {
                IMMDevice device;
                deviceCollection.Item(i, out device);

                IPropertyStore propertyStore;
                device.OpenPropertyStore(StorageAccessMode.Read, out propertyStore);

                PropertyKey propertyKey = PropertyKeys.PKEY_DEVICE_FRIENDLY_NAME;
                PropVariant propVariant;

                propertyStore.GetValue(ref propertyKey, out propVariant);

                object propValue = propVariant.Value;
                if (propValue.ToString() == deviceFriendlyName)
                {
                    targetDevice = device;

                    Marshal.ReleaseComObject(propertyStore);
                    break;
                }

                Marshal.ReleaseComObject(device);
                Marshal.ReleaseComObject(propertyStore);
            }

            Marshal.ReleaseComObject(deviceEnumerator);
            Marshal.ReleaseComObject(deviceCollection);
            return targetDevice;
        }

        /// <summary>
        /// tries to get an instance of <see cref="ISimpleAudioVolume"/> by its process-id
        /// </summary>
        /// <param name="pProcessId">applications process-id</param>
        /// <param name="pDeviceName">sound output device name</param>
        /// <param name="oISimpleAudioVolume">instance of <see cref="ISimpleAudioVolume"/></param>
        /// <returns>true if succesful, false if not</returns>
        private static bool TryGetISimpleAudioVolumeByProcessId(int pProcessId, string pDeviceName, out ISimpleAudioVolume oISimpleAudioVolume)
        {
            oISimpleAudioVolume = null;

            IMMDevice targetDevice = GetIMMDeviceByName(pDeviceName);
            if (targetDevice == null)
            {
                return false;
            }

            Guid IID_IAudioSessionManager2 = typeof(IAudioSessionManager2).GUID;
            object o;
            targetDevice.Activate(ref IID_IAudioSessionManager2, 0, IntPtr.Zero, out o);
            IAudioSessionManager2 mgr = (IAudioSessionManager2)o;

            IAudioSessionEnumerator sessionEnumerator;
            mgr.GetSessionEnumerator(out sessionEnumerator);
            int count;
            sessionEnumerator.GetCount(out count);

            ISimpleAudioVolume volumeControl = null;
            for (int i = 0; i < count; i++)
            {
                IAudioSessionControl2 ctl;
                sessionEnumerator.GetSession(i, out ctl);
                int cpid;
                ctl.GetProcessId(out cpid);

                if (cpid == pProcessId)
                {
                    volumeControl = ctl as ISimpleAudioVolume;
                    oISimpleAudioVolume = volumeControl;
                    break;
                }
                Marshal.ReleaseComObject(ctl);
            }
            Marshal.ReleaseComObject(sessionEnumerator);
            Marshal.ReleaseComObject(mgr);
            Marshal.ReleaseComObject(targetDevice);
            
            return volumeControl != null;
        }

        
    }
}
