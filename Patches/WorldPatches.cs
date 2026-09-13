using HarmonyLib;
using VersaValheimHacks.Features;

namespace VersaValheimHacks.Patches
{
    internal static class WorldPatches
    {
        [HarmonyPatch(typeof(World), MethodType.Constructor)]
        internal class World_Constructor
        {
            private static void Postfix(World __instance) => DebugTools.OnWorldInstantiated(__instance);
        }

        [HarmonyPatch(typeof(World), MethodType.Constructor, new[] { typeof(SaveWithBackups), typeof(World.SaveDataError) })]
        internal class World_ConstructorFromSave
        {
            private static void Postfix(World __instance) => DebugTools.OnWorldInstantiated(__instance);
        }

        [HarmonyPatch(typeof(World), MethodType.Constructor, new[] { typeof(string), typeof(string) })]
        internal class World_ConstructorNamed
        {
            private static void Postfix(World __instance) => DebugTools.OnWorldInstantiated(__instance);
        }
    }
}
