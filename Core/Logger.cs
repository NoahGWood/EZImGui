using Serilog;
using Serilog.Sinks;

namespace EZImGui.Core
{
    public static class Logger
    {
        private static readonly ILogger CoreLog = new LoggerConfiguration()
            .MinimumLevel.Error()
            .WriteTo.File("logs/core.debug", rollingInterval: RollingInterval.Infinite)
            .CreateLogger();
        private static readonly ILogger AppDebugLog = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File("logs/" + App.Name + ".debug", rollingInterval: RollingInterval.Infinite)
            .CreateLogger();
        public static void Error(Exception ex, string message)
        {
            AppDebugLog.Error(ex, message);
        }
        public static void Debug(string message)
        {
            AppDebugLog.Debug(message);
        }
        public static void CoreExcept(Exception ex, string message)
        {
            CoreLog.Error(ex, message);
        }
        public static void CoreDebug(string message)
        {
            CoreLog.Debug(message);
            Console.WriteLine(message);
        }
    }
}
