using HarmonyLib;
using VersaValheimHacks.Features;

namespace VersaValheimHacks.Patches
{
    internal static class ZoneSystemPatches
    {
        [HarmonyPatch(typeof(ZoneSystem), MethodType.Constructor)]
        internal class ZoneSystem_Constructor
        {
            private static void Postfix(ZoneSystem __instance) => DebugTools.OnZoneSystemInstantiated(__instance);
        }

        [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GetGlobalKey), new[] { typeof(GlobalKeys) })]
        internal class ZoneSystem_GetGlobalKey
        {
            private static void Postfix(GlobalKeys key, ref bool __result) => FreeCrafting.ForceGlobalKey(key, ref __result);
        }
    }
}
