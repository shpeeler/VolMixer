namespace VolMixerConsole.Logging
{
    internal class LoggerInfo
    {
        public bool WarningEnabled { get; } = false;

        public bool InfoEnabled { get; } = false;

        public bool ErrorEnabled { get; } = false;

        public LoggerInfo(LogLevel pLogLevel)
        {
            if ((pLogLevel & LogLevel.ERROR) == LogLevel.ERROR)
            {
                ErrorEnabled = true;
            }

            if ((pLogLevel & LogLevel.INFO) == LogLevel.INFO)
            {
                InfoEnabled = true;
            }

            if ((pLogLevel & LogLevel.WARNING) == LogLevel.WARNING)
            {
                WarningEnabled = true;
            }
        }
    }
}
