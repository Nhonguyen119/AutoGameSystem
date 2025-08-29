using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Serilog;
using Serilog.Events;


namespace AutoGameSystem.Utilities
{
    public static class Logger
    {
        private static ILogger _logger;

        static Logger()
        {
            var logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            if (!Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory);

            _logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        public static void Info(string message)
        {
            _logger.Information(message);
        }

        public static void Warning(string message)
        {
            _logger.Warning(message);
        }

        public static void Error(string message)
        {
            _logger.Error(message);
        }

        public static void Debug(string message)
        {
            _logger.Debug(message);
        }
    }
}
