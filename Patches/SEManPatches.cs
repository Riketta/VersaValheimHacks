using HarmonyLib;

using System.Collections.Generic;
using VersaValheimHacks.Features;



namespace VersaValheimHacks.Patches

{

    [HarmonyPatch(typeof(SEMan), nameof(SEMan.ModifyHealthRegen))]

    internal class SEMan_ModifyHealthRegen

    {

        private static void Postfix(ref float regenMultiplier) => BetterEating.ScaleHealthRegen(ref regenMultiplier);

    }


    /// <summary>

    /// Keeps the stacked extra powers out of the HUD status row - icons

    /// only, the status effects themselves stay active.

    /// </summary>

    [HarmonyPatch(typeof(SEMan), nameof(SEMan.GetHUDStatusEffects))]

    internal class SEMan_GetHUDStatusEffects

    {

        private static void Postfix(List<StatusEffect> effects) => BetterPowers.HideExtraPowerVisuals(effects);
    }
}
