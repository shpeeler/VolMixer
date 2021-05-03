using System;
using System.Runtime.InteropServices;
using VolMixerConsole.CoreAudio.Enums;

namespace VolMixerConsole.CoreAudio.Interfaces
{
    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDeviceEnumerator
    {
        [PreserveSig]
        int EnumAudioEndpoints(
            [In][MarshalAs(UnmanagedType.I4)] EDataFlow dataFlow,
            [In][MarshalAs(UnmanagedType.U4)] EDeviceState stateMask,
            [Out][MarshalAs(UnmanagedType.Interface)] out IMMDeviceCollection devices);

        [PreserveSig]
        int GetDefaultAudioEndpoint(
            [In][MarshalAs(UnmanagedType.I4)] EDataFlow dataFlow,
            [In][MarshalAs(UnmanagedType.I4)] ERole role,
            [Out][MarshalAs(UnmanagedType.Interface)] out IMMDevice device);

        [PreserveSig]
        int GetDevice(
            [In][MarshalAs(UnmanagedType.LPWStr)] string endpointId,
            [Out][MarshalAs(UnmanagedType.Interface)] out IMMDevice device);
    }
}
