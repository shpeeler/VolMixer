using System;
using System.Runtime.InteropServices;

namespace VolMixerConsole.CoreAudio.Interfaces
{
    [Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDeviceCollection
    {
        [PreserveSig]
        int GetCount([Out][MarshalAs(UnmanagedType.U4)] out uint count);

        [PreserveSig]
        int Item(
            [In][MarshalAs(UnmanagedType.U4)] uint index,
            [Out][MarshalAs(UnmanagedType.Interface)] out IMMDevice device);
    }
}
