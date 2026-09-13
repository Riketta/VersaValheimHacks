using HarmonyLib;
using VersaValheimHacks.Features;

namespace VersaValheimHacks.Patches
{
    [HarmonyPatch(typeof(Minimap), "Start")]
    internal class Minimap_Start
    {
        private static void Postfix(ref float ___m_exploreRadius) => MapReveal.ApplyRadiusMultiplier(ref ___m_exploreRadius);
    }
}
