using HarmonyLib;
using VersaValheimHacks.Features;

namespace VersaValheimHacks.Patches
{
    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.GiveDefaultItems))]
    internal class Humanoid_GiveDefaultItems
    {
        private static bool Prefix(Humanoid __instance) => SkeletonMinions.OverrideDefaultItems(__instance);
    }
}
