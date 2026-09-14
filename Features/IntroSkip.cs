namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Auto-skips the first-spawn intro: video cinematic, Valkyrie flight and
    /// intro text. Uses the game's own SkipIntro path right after the intro
    /// state activates, so spawning falls back to the normal ground spawn.
    /// </summary>
    internal static class IntroSkip
    {
        /// <summary>
        /// Blocks intro videos at the single choke point used by both the
        /// launch cinematic (FejdStartup main menu) and the world-entry intro
        /// (Game.Update). Returning false makes Play() report "not played",
        /// which both callers handle gracefully.
        /// </summary>
        public static bool AllowVideo(CinematicsManager.Settings setting)
        {
            if (!GlobalState.Config.SkipIntroCinematic || setting != CinematicsManager.Settings.Intro)
                return true;

            HarmonyLog.Log("[IntroSkip] Blocking intro video.");
            return false;
        }

        public static void TrySkip(Game game)
        {
            if (game is null || !GlobalState.Config.SkipIntroCinematic)
                return;

            // Only once the intro is actually active (not while still queued
            // behind the loading screen) - by then all UI instances exist.
            if (!game.InIntro())
                return;

            HarmonyLog.Log("[IntroSkip] Skipping start cinematic.");

            // The intro video starts inside Game.Update, the same frame this
            // postfix runs - stop it before SkipIntro (which doesn't stop it).
            if (CinematicsManager.s_instance != null && CinematicsManager.IsStartedPlaying())
                CinematicsManager.Stop();

            game.SkipIntro();
        }
    }
}
