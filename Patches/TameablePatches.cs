using HarmonyLib;
using VersaValheimHacks.Features;

namespace VersaValheimHacks.Patches
{
    [HarmonyPatch(typeof(Tameable), "UnsummonMaxInstances")]
    internal class Tameable_UnsummonMaxInstances
    {
        private static void Prefix(ref int maxInstances) => SkeletonMinions.OverrideSummonLimit(ref maxInstances);
    }
}
