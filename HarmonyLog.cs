using HarmonyLib;
using System;
using System.Diagnostics;

namespace VersaValheimHacks
{
    internal static class HarmonyLog
    {
        public static void Log(string message)
        {
#if DEBUG
            if (GlobalState.Config != null && GlobalState.Config.Logging)
                FileLog.Log($"[{DateTime.Now:HH:mm:ss.fffffff}] {message}");
#endif
        }

        public static void DumpStackTrace()
        {
#if DEBUG
            var stackTrace = new StackTrace();
            Log("Stack Trace:");
            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                var callerMethod = stackTrace.GetFrame(i).GetMethod();
                Log($"  > [{i}] {callerMethod.ReflectedType.FullName}.{callerMethod.Name}");
            }
#endif
        }
    }
}
