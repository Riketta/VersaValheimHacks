using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace VersaValheimHacks
{
    /// <summary>
    /// Own file logger. Harmony's FileLog buffers every line until a log
    /// listener (e.g. BepInEx) attaches - with this loader none ever does,
    /// so anything written through it is silently lost. Lines are appended
    /// to VersaValheimHacks.log in the game root (same place as the config).
    /// Boot messages (before the config is read, or when it is unreadable)
    /// always land; afterwards the Logging config flag gates them.
    /// </summary>
    internal static class HarmonyLog
    {
        private const string LogFileName = "VersaValheimHacks.log";

        private static readonly object WriteLock = new object();
        private static string _logFilePath;

        private static string LogFilePath
        {
            get
            {
                if (_logFilePath is null)
                {
                    try { _logFilePath = Path.GetFullPath(LogFileName); }
                    catch { _logFilePath = LogFileName; }
                }
                return _logFilePath;
            }
        }

        private static bool IsEnabled => GlobalState.Config is null || GlobalState.Config.Logging;

        public static void Log(string message)
        {
            if (!IsEnabled)
                return;

            string line = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
            lock (WriteLock)
            {
                try
                {
                    File.AppendAllText(LogFilePath, line + Environment.NewLine);
                }
                catch
                {
                    // Logging must never throw into game or patch code.
                }
            }
        }

        public static void DumpStackTrace()
        {
            var stackTrace = new StackTrace();
            Log("Stack Trace:");
            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                var callerMethod = stackTrace.GetFrame(i).GetMethod();
                Log($"  > [{i}] {callerMethod.ReflectedType.FullName}.{callerMethod.Name}");
            }
        }
    }
}
