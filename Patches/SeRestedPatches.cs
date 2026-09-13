using HarmonyLib;
using VersaValheimHacks.Features;

namespace VersaValheimHacks.Patches
{
    [HarmonyPatch(typeof(SE_Rested), nameof(SE_Rested.Setup))]
    internal class SeRested_Setup
    {
        private static void Prefix(SE_Rested __instance) => RestedBuff.ApplyDurationOverrides(__instance);
    }
}
