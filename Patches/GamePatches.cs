using HarmonyLib;
using VersaValheimHacks.Features;

namespace VersaValheimHacks.Patches
{
    internal static class GamePatches
    {
        [HarmonyPatch(typeof(Game), "Update")]
        internal class Game_Update
        {
            private static void Postfix(Game __instance) => IntroSkip.TrySkip(__instance);
        }
    }
}
