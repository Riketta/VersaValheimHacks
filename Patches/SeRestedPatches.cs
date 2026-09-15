using HarmonyLib;
using VersaValheimHacks.Features;

namespace VersaValheimHacks.Patches
{
    [HarmonyPatch(typeof(SE_Rested), nameof(SE_Rested.Setup))]
    internal class SeRested_Setup
    {
        private static void Prefix(SE_Rested __instance) => RestedBuff.ApplyDurationOverrides(__instance);
    }

    // Vanilla auto-reset: re-resting re-arms rested to full duration.
    [HarmonyPatch(typeof(SE_Rested), nameof(SE_Rested.ResetTime))]
    internal class SeRested_ResetTime
    {
        private static bool Prefix() => RestedBuff.AllowAutoRefresh;
    }
}
