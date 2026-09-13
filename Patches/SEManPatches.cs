using HarmonyLib;
using VersaValheimHacks.Features;

namespace VersaValheimHacks.Patches
{
    [HarmonyPatch(typeof(SEMan), nameof(SEMan.ModifyHealthRegen))]
    internal class SEMan_ModifyHealthRegen
    {
        private static void Postfix(ref float regenMultiplier) => BetterEating.ScaleHealthRegen(ref regenMultiplier);
    }
}
