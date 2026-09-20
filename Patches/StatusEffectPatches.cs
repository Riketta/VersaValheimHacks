using HarmonyLib;
using VersaValheimHacks.Features;

namespace VersaValheimHacks.Patches
{
    /// <summary>
    /// Suppresses the start message (e.g. "Power of ... breaks free" in the
    /// screen center) of stacked extra powers, so messages and HUD icons are
    /// hidden together while the effects stay active. The message field is
    /// blanked only for the duration of Setup and restored afterwards.
    /// </summary>
    [HarmonyPatch(typeof(StatusEffect), "Setup")]
    internal class StatusEffect_Setup
    {
        private static void Prefix(StatusEffect __instance, out string __state)
        {
            __state = null;
            if (BetterPowers.IsHiddenExtraPower(__instance))
            {
                __state = __instance.m_startMessage;
                __instance.m_startMessage = "";
            }
        }

        private static void Postfix(StatusEffect __instance, string __state)
        {
            if (__state != null)
                __instance.m_startMessage = __state;
        }
    }
}
