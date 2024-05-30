using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaValheimHacks
{
    internal class HarmonyLog
    {
        public static void Log(string message)
        {
#if DEBUG
            if (GlobalState.Config != null && GlobalState.Config.Logging)
                FileLog.Log($"[{DateTime.Now:HH:mm:ss.fffffff}] {message}");
#endif
        }

        public static string GetCallingClassAndMethodNames()
        {
            var method = new StackTrace().GetFrame(3).GetMethod();
            var className = method.ReflectedType.Name;

            return $"{className}.{method.Name}()";
        }

        public static void DumpStackTrace()
        {
#if DEBUG
            var stackTrace = new StackTrace();
            Log($"Stack Trace:");
            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                var callerMethod = stackTrace.GetFrame(i).GetMethod();
                Log($"  > [{i}] {$"{callerMethod.ReflectedType.Namespace}.{callerMethod.ReflectedType.Name}.{callerMethod.Name}"}");
            }
#endif
        }
    }
}
