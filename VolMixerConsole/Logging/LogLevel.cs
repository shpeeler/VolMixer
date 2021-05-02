using System;

namespace VolMixerConsole.Logging
{
    [Flags]
    public enum LogLevel : byte
    {
        NONE = 0,
        INFO = 1,
        WARNING = 2,
        ERROR = 4
    }
}
