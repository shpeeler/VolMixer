using System;

namespace VolMixerConsole.Logging
{
    public interface ILog
    {
        void LogError(string pMessage);

        void LogError(string pMessage, Exception pException);

        void LogError(string pMessage, string pParam1);

        void LogError(string pMessage, string pParam1, string pParam2);

        void LogInfo(string pMessage);

        void LogInfo(string pMessage, Exception pException);

        void LogInfo(string pMessage, string pParam1);

        void LogInfo(string pMessage, string pParam1, string pParam2);

        void LogWarning(string pMessage);

        void LogWarning(string pMessage, Exception pException);

        void LogWarning(string pMessage, string pParam1);

        void LogWarning(string pMessage, string pParam1, string pParam2);
    }
}
