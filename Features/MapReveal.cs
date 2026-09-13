namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Increases the exploration fog reveal radius around the player
    /// (vanilla 100). Applied once per Minimap instance (Start postfix).
    /// Streamer mode keeps the vanilla radius.
    /// </summary>
    internal static class MapReveal
    {
        public static void ApplyRadiusMultiplier(ref float exploreRadius)
        {
            if (GlobalState.Config.StreamerMode)
                return;

            float multiplier = GlobalState.Config.GodModeOptions.MapRevealRadiusMultiplier;
            if (multiplier > 1f)
                exploreRadius *= multiplier;
        }
    }
}
