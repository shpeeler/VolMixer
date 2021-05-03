using System;
using System.Runtime.InteropServices;
using log4net;
using VolMixerConsole.CoreAudio;
using VolMixerConsole.CoreAudio.Properties;
using VolMixerConsole.CoreAudio.Enums;
using VolMixerConsole.CoreAudio.Interfaces;



namespace VolMixerConsole
{
    public class VolMixerHelper
    {
        readonly ILog log;

        public VolMixerHelper(ILog pLog)
        {

        }

        public bool TryGetApplicationVolume(int pProcessId,  string pDeviceName, out float? oVolume)
        {
            oVolume = null;

            ISimpleAudioVolume simpleAudioVolume;
            if(GetVolumeObject(pProcessId, pDeviceName, out simpleAudioVolume) == false)
            {
                return false;
            }

            float level;
            simpleAudioVolume.GetMasterVolume(out level);

            Marshal.ReleaseComObject(simpleAudioVolume);

            oVolume = level * 100;
            return true;
        }
        
        public bool TrySetApplicationVolume(int pProcessId, string pDeviceName, float pVolumeLevel)
        {
            ISimpleAudioVolume simpleAudioVolume;
            if (GetVolumeObject(pProcessId, pDeviceName, out simpleAudioVolume) == false)
            {
                return false;
            }

            Guid guid = Guid.Empty;
            simpleAudioVolume.SetMasterVolume(pVolumeLevel / 100, ref guid);

            Marshal.ReleaseComObject(simpleAudioVolume);
            return true;
        }

        private bool GetVolumeObject(int pProcessId, string pDeviceName, out ISimpleAudioVolume oISimpleAudioVolume)
        {
            oISimpleAudioVolume = null;

            // get the speakers (1st render + multimedia) device
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
                if (propValue.ToString() == pDeviceName)
                {
                    targetDevice = device;

                    Marshal.ReleaseComObject(propertyStore);
                    break;
                }

                Marshal.ReleaseComObject(device);
                Marshal.ReleaseComObject(propertyStore);
            }

            if (targetDevice == null)
            {
                Marshal.ReleaseComObject(deviceEnumerator);
                return false;
            }

            // activate the session manager. we need the enumerator
            Guid IID_IAudioSessionManager2 = typeof(IAudioSessionManager2).GUID;
            object o;
            targetDevice.Activate(ref IID_IAudioSessionManager2, 0, IntPtr.Zero, out o);
            IAudioSessionManager2 mgr = (IAudioSessionManager2)o;

            // enumerate sessions for on this device
            IAudioSessionEnumerator sessionEnumerator;
            mgr.GetSessionEnumerator(out sessionEnumerator);
            int count;
            sessionEnumerator.GetCount(out count);

            // search for an audio session with the required name
            // NOTE: we could also use the process id instead of the app name (with IAudioSessionControl2)
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
            Marshal.ReleaseComObject(deviceEnumerator);
            
            return volumeControl != null;
        }
    }
}
