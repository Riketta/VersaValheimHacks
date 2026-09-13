using HarmonyLib;
using VersaValheimHacks.Features;

namespace VersaValheimHacks.Patches
{
    [HarmonyPatch(typeof(Hud), "Update")]
    internal class Hud_Update
    {
        private static void Postfix(Hud __instance) => ShieldBar.UpdateBar(__instance);
    }
}
