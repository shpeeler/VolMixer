using System;
using System.Runtime.InteropServices;
using VolMixerConsole.CoreAudio.Enums;

namespace VolMixerConsole.CoreAudio.Interfaces
{
    [Guid("D666063F-1587-4E43-81F1-B948E807363F")] 
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDevice
    {
        [PreserveSig]
        int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);

        [PreserveSig]
        int OpenPropertyStore(
            [In][MarshalAs(UnmanagedType.U4)] StorageAccessMode accessMode,
            [Out][MarshalAs(UnmanagedType.Interface)] out IPropertyStore properties);
    }
}
