using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static World;

namespace VersaValheimHacks.Patches
{
    internal class Debug
    {
        [HarmonyPatch(typeof(ZoneSystem), MethodType.Constructor)]
        internal class GetZoneSystemInstance
        {
            private const string Prefix = "ZoneSystem.Constructor";

            [HarmonyPostfix]
            public static void Constructor(ZoneSystem __instance)
            {
                if (!GlobalState.EnableDebugTools)
                    return;

                HarmonyLog.Log($"[{Prefix}.Postfix] New ZoneSystem instance.");
                DebugTools.OnZoneSystemInstantiated(__instance);
            }
        }

        [HarmonyPatch(typeof(World), MethodType.Constructor, new Type[] { })]
        internal class GetWorldInstanceA
        {
            private const string Prefix = "World.ConstructorA";

            [HarmonyPostfix]
            public static void Constructor(World __instance)
            {
                if (!GlobalState.EnableDebugTools)
                    return;

                HarmonyLog.Log($"[{Prefix}.Postfix] New World instance.");
                DebugTools.OnWorldInstantiated(__instance);
            }
        }

        [HarmonyPatch(typeof(World), MethodType.Constructor, new Type[] { typeof(SaveWithBackups), typeof(SaveDataError) })]
        internal class GetWorldInstanceB
        {
            private const string Prefix = "World.ConstructorB";

            [HarmonyPostfix]
            public static void Constructor(World __instance)
            {
                if (!GlobalState.EnableDebugTools)
                    return;

                HarmonyLog.Log($"[{Prefix}.Postfix] New World instance.");
                DebugTools.OnWorldInstantiated(__instance);
            }
        }

        [HarmonyPatch(typeof(World), MethodType.Constructor, new Type[] { typeof(string), typeof(string) })]
        internal class GetWorldInstanceC
        {
            private const string Prefix = "World.ConstructorC";

            [HarmonyPostfix]
            public static void Constructor(World __instance)
            {
                if (!GlobalState.EnableDebugTools)
                    return;

                HarmonyLog.Log($"[{Prefix}.Postfix] New World instance.");
                DebugTools.OnWorldInstantiated(__instance);
            }
        }
    }
}
