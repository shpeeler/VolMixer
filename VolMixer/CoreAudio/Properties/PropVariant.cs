using System;
using System.Runtime.InteropServices;

namespace VolMixer.CoreAudio.Properties
{
    [StructLayout(LayoutKind.Explicit)]
    public struct PropVariant
    {
        [FieldOffset(0)]
        private short vt;
        [FieldOffset(2)]
        private readonly short wReserved1;
        [FieldOffset(4)]
        private readonly short wReserved2;
        [FieldOffset(6)]
        private readonly short wReserved3;
        [FieldOffset(8)]
        private readonly sbyte cVal;
        [FieldOffset(8)]
        private readonly byte bVal;
        [FieldOffset(8)]
        private readonly short iVal;
        [FieldOffset(8)]
        private readonly ushort uiVal;
        [FieldOffset(8)]
        private readonly int lVal;
        [FieldOffset(8)]
        private readonly uint ulVal;
        [FieldOffset(8)]
        private readonly int intVal;
        [FieldOffset(8)]
        private readonly uint uintVal;
        [FieldOffset(8)]
        private long hVal;
        [FieldOffset(8)]
        private readonly long uhVal;
        [FieldOffset(8)]
        private readonly float fltVal;
        [FieldOffset(8)]
        private readonly double dblVal;
        [FieldOffset(8)]
        private readonly bool boolVal;
        [FieldOffset(8)]
        private readonly int scode;
        //CY cyVal;
        [FieldOffset(8)]
        private readonly DateTime date;
        [FieldOffset(8)]
        private readonly System.Runtime.InteropServices.ComTypes.FILETIME filetime;
        //CLSID* puuid;
        //CLIPDATA* pclipdata;
        //BSTR bstrVal;
        //BSTRBLOB bstrblobVal;
        //LPSTR pszVal;
        [FieldOffset(8)]
        private IntPtr pointerValue; //LPWSTR 
        //IUnknown* punkVal;
        /*IDispatch* pdispVal;
        IStream* pStream;
        IStorage* pStorage;
        LPVERSIONEDSTREAM pVersionedStream;
        LPSAFEARRAY parray;
        CAC cac;
        CAUB caub;
        CAI cai;
        CAUI caui;
        CAL cal;
        CAUL caul;
        CAH cah;
        CAUH cauh;
        CAFLT caflt;
        CADBL cadbl;
        CABOOL cabool;
        CASCODE cascode;
        CACY cacy;
        CADATE cadate;
        CAFILETIME cafiletime;
        CACLSID cauuid;
        CACLIPDATA caclipdata;
        CABSTR cabstr;
        CABSTRBLOB cabstrblob;
        CALPSTR calpstr;
        CALPWSTR calpwstr;
        CAPROPVARIANT capropvar;
        CHAR* pcVal;
        UCHAR* pbVal;
        SHORT* piVal;
        USHORT* puiVal;
        LONG* plVal;
        ULONG* pulVal;
        INT* pintVal;
        UINT* puintVal;
        FLOAT* pfltVal;
        DOUBLE* pdblVal;
        VARIANT_BOOL* pboolVal;
        DECIMAL* pdecVal;
        SCODE* pscode;
        CY* pcyVal;
        DATE* pdate;
        BSTR* pbstrVal;
        IUnknown** ppunkVal;
        IDispatch** ppdispVal;
        LPSAFEARRAY* pparray;
        PROPVARIANT* pvarVal;
        */

        /// <summary>
        ///     Creates a new PropVariant containing a long value
        /// </summary>
        public static PropVariant FromLong(long value)
        {
            return new PropVariant { vt = (short)VarEnum.VT_I8, hVal = value };
        }

        /// <summary>
        ///     Gets the type of data in this PropVariant
        /// </summary>
        public VarEnum DataType => (VarEnum)vt;

        /// <summary>
        ///     Property value
        /// </summary>
        public object Value
        {
            get
            {
                var ve = DataType;
                switch (ve)
                {
                    case VarEnum.VT_I1:
                        return bVal;
                    case VarEnum.VT_I2:
                        return iVal;
                    case VarEnum.VT_I4:
                        return lVal;
                    case VarEnum.VT_I8:
                        return hVal;
                    case VarEnum.VT_INT:
                        return iVal;
                    case VarEnum.VT_UI4:
                        return ulVal;
                    case VarEnum.VT_UI8:
                        return uhVal;
                    case VarEnum.VT_BOOL:
                        return boolVal;
                    case VarEnum.VT_LPWSTR:
                        return Marshal.PtrToStringUni(pointerValue);
                    case VarEnum.VT_CLSID:
                        return (Guid)Marshal.PtrToStructure(pointerValue, typeof(Guid));
                }
                throw new NotSupportedException("PropVariant " + ve);
            }
            set
            {
                var s = value as string;

                if (s != null && DataType == VarEnum.VT_LPWSTR)
                {
                    pointerValue = Marshal.StringToBSTR(s);
                }
            }
        }

        public bool IsSupported()
        {
            switch (DataType)
            {
                case VarEnum.VT_I1:
                    return true;
                case VarEnum.VT_I2:
                    return true;
                case VarEnum.VT_I4:
                    return true;
                case VarEnum.VT_I8:
                    return true;
                case VarEnum.VT_INT:
                    return true;
                case VarEnum.VT_UI4:
                    return true;
                case VarEnum.VT_UI8:
                    return true;
                case VarEnum.VT_BOOL:
                    return true;
                case VarEnum.VT_LPWSTR:
                    return true;
                case VarEnum.VT_CLSID:
                    return true;
                default:
                    return false;
            }
        }
    }
}
