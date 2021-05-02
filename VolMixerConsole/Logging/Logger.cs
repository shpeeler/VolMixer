using System;
using System.IO;

namespace VolMixerConsole.Logging
{
    public class Logger : ILog
    {
        private const string DEFAULT_PATH = @"C:\temp";
        private const string ERROR_PREFIX = "ERROR - ";
        private const string INFO_PREFIX = "INFO - ";
        private const string WARN_PREFIX = "WARNING - ";
        private const string LOG_ENDING = ".log";

        private readonly LoggerInfo _LoggerInfo;
        private readonly string _LogFilePath;

        public Logger(string pLogFileName, string pDirectory = DEFAULT_PATH, LogLevel pLogLevel = LogLevel.ERROR | LogLevel.INFO | LogLevel.WARNING)
        {
            if (string.IsNullOrEmpty(pLogFileName))
            {
                throw new ArgumentNullException(nameof(pLogFileName));
            }

            if (pDirectory.EndsWith("\\") == false)
            {
                pDirectory += "\\";
            }

            if (Directory.Exists(pDirectory) == false)
            {
                throw new DirectoryNotFoundException("The directory: '{pDirectory}' does not exist.");
            }

            string logFileName = pLogFileName + LOG_ENDING;
            _LogFilePath = pDirectory + logFileName;
            _LoggerInfo = new LoggerInfo(pLogLevel);
        }
        
        private void Write(string pMessage)
        {
            string message = DateTime.Now + " - " + pMessage;
            using (StreamWriter writer = File.AppendText(_LogFilePath))
            {
                writer.WriteLine(message);
            }
        }

        public void LogError(string pMessage)
        {
            if (_LoggerInfo.ErrorEnabled == false)
                return;

            Write(ERROR_PREFIX + pMessage);
        }

        public void LogError(string pMessage, Exception pException)
        {
            if (_LoggerInfo.ErrorEnabled == false)
                return;

            Write(ERROR_PREFIX + $"Message: {pMessage} Exception: {pException}");
        }

        public void LogError(string pMessage, string pParam1)
        {
            if (_LoggerInfo.ErrorEnabled == false)
                return;

            Write(ERROR_PREFIX + string.Format(pMessage, pParam1));
        }

        public void LogError(string pMessage, string pParam1, string pParam2)
        {
            if (_LoggerInfo.ErrorEnabled == false)
                return;

            Write(ERROR_PREFIX + string.Format(pMessage, pParam1, pParam2));
        }

        public void LogInfo(string pMessage)
        {
            if (_LoggerInfo.InfoEnabled == false)
                return;

            Write(INFO_PREFIX + pMessage);
        }

        public void LogInfo(string pMessage, Exception pException)
        {
            if (_LoggerInfo.InfoEnabled == false)
                return;

            Write(INFO_PREFIX + $"Message: {pMessage} Exception: {pException}");
        }

        public void LogInfo(string pMessage, string pParam1)
        {
            if (_LoggerInfo.InfoEnabled == false)
                return;

            Write(INFO_PREFIX + string.Format(pMessage, pParam1));
        }

        public void LogInfo(string pMessage, string pParam1, string pParam2)
        {
            if (_LoggerInfo.InfoEnabled == false)
                return;

            Write(INFO_PREFIX + string.Format(pMessage, pParam1, pParam2));
        }

        public void LogWarning(string pMessage)
        {
            if (_LoggerInfo.WarningEnabled == false)
                return;

            Write(WARN_PREFIX + pMessage);
        }

        public void LogWarning(string pMessage, Exception pException)
        {
            if (_LoggerInfo.WarningEnabled == false)
                return;

            Write(WARN_PREFIX + $"Message: {pMessage} Exception: {pException}");
        }

        public void LogWarning(string pMessage, string pParam1)
        {
            if (_LoggerInfo.WarningEnabled == false)
                return;

            Write(WARN_PREFIX + string.Format(pMessage, pParam1));
        }

        public void LogWarning(string pMessage, string pParam1, string pParam2)
        {
            if (_LoggerInfo.WarningEnabled == false)
                return;

            Write(WARN_PREFIX + string.Format(pMessage, pParam1, pParam2));
        }
    }
}
