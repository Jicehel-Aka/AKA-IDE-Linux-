using System;

namespace GamebuinoAKA.Core.Services
{
    public static class Log
    {
        private static ILogService? _service;
        public static void Configure(ILogService service) => _service = service;
        public static bool IsConfigured => _service != null;
        public static string LogFolder => _service?.LogFolder ?? string.Empty;
        public static string LogFilePath => _service?.LogFilePath ?? string.Empty;
        public static void Info(string message) => _service?.Info(message);
        public static void Warn(string message) => _service?.Warn(message);
        public static void Error(string message, Exception? ex = null) => _service?.Error(message, ex);
        public static bool Clear() => _service?.Clear() ?? false;
    }
}
