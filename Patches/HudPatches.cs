using HarmonyLib;

using VersaValheimHacks.Features;



namespace VersaValheimHacks.Patches

{

    [HarmonyPatch(typeof(Hud), "Update")]

    internal class Hud_Update

    {

        private static void Postfix(Hud __instance)

        {

            ShieldBar.UpdateBar(__instance);



            HealthRegenCountdown.UpdateLabel(__instance);



            SkeletonCommand.DebugProbe();

        }

    }


    [HarmonyPatch(typeof(Hud), "UpdateStamina")]
    internal class Hud_UpdateStamina
    {
        private static void Postfix(Hud __instance) => EnergyBarsOffset.UpdateStamina(__instance);
    }

    [HarmonyPatch(typeof(Hud), "UpdateAdrenaline")]
    internal class Hud_UpdateAdrenaline
    {
        private static void Postfix(Hud __instance) => EnergyBarsOffset.UpdateAdrenaline(__instance);
    }

    [HarmonyPatch(typeof(Hud), "UpdateEitr")]
    internal class Hud_UpdateEitr
    {
        private static void Postfix(Hud __instance) => EnergyBarsOffset.UpdateEitr(__instance);
    }
}
