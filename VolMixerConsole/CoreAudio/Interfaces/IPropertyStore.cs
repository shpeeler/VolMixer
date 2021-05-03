using System;
using System.Runtime.InteropServices;
using VolMixerConsole.CoreAudio.Properties;

namespace VolMixerConsole.CoreAudio.Interfaces
{
    [Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPropertyStore
    {
        [PreserveSig]
        int GetCount(
            [Out][MarshalAs(UnmanagedType.U4)] out uint propertyCount);

        [PreserveSig]
        int GetAt(
            [In][MarshalAs(UnmanagedType.U4)] uint propertyIndex,
            [Out] out PropertyKey propertyKey);

        [PreserveSig]
        int GetValue(
            [In] ref PropertyKey propertyKey,
            [Out] out PropVariant value);

        [PreserveSig]
        int SetValue(
            [In] ref PropertyKey propertyKey,
            [In] ref object value);

        [PreserveSig]
        int Commit();
    }
}
