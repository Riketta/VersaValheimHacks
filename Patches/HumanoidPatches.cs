using HarmonyLib;
using VersaValheimHacks.Features;

namespace VersaValheimHacks.Patches
{
    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.GiveDefaultItems))]
    internal class Humanoid_GiveDefaultItems
    {
        private static bool Prefix(Humanoid __instance) => SkeletonMinions.OverrideDefaultItems(__instance);
    }

    // BlockAttack is protected (string target). It only reads m_blockTimer,
    // so scaling it in the prefix and restoring in the postfix changes the
    // effective parry window without touching anything else.
    [HarmonyPatch(typeof(Humanoid), "BlockAttack")]
    internal class Humanoid_BlockAttack
    {
        private static void Prefix(ref float ___m_blockTimer) => ParryTuning.ScaleBlockTimer(ref ___m_blockTimer);

        private static void Postfix(ref float ___m_blockTimer) => ParryTuning.RestoreBlockTimer(ref ___m_blockTimer);
    }
}
