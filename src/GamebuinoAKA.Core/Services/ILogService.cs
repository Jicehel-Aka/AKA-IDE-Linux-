using System;

namespace GamebuinoAKA.Core.Services
{
    public interface ILogService
    {
        string LogFolder { get; }
        string LogFilePath { get; }
        void Info(string message);
        void Warn(string message);
        void Error(string message, Exception? ex = null);
        bool Clear();
    }
}
