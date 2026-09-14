using HarmonyLib;
using System;
using VersaValheimHacks.Features;

namespace VersaValheimHacks.Patches
{
    internal static class GamePatches
    {
        // Targets the Play(Settings, VideoCompleteAction) overload specifically
        // (Play is overloaded three times) - the intro funnel used by both the
        // launch menu cinematic and the world-entry intro.
        [HarmonyPatch(typeof(CinematicsManager), nameof(CinematicsManager.Play),
            new Type[] { typeof(CinematicsManager.Settings), typeof(CinematicsManager.VideoCompleteAction) })]
        internal class Play
        {
            private static bool Prefix(CinematicsManager.Settings firstWithSetting) => IntroSkip.AllowVideo(firstWithSetting);
        }

        [HarmonyPatch(typeof(Game), "Update")]
        internal class Game_Update
        {
            private static void Postfix(Game __instance) => IntroSkip.TrySkip(__instance);
        }
    }
}
