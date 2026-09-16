namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Widens or narrows the perfect-block (parry) timing window. Vanilla
    /// counts a block as perfect when it was raised less than 0.25 s before
    /// the hit lands (hardcoded comparison in Humanoid.BlockAttack). The
    /// prefix divides the block timer by the configured multiplier before
    /// the original check runs - so the effective window becomes
    /// 0.25 s x multiplier - and the postfix restores the real timer.
    /// Only engages while the master toggle is on.
    /// </summary>
    internal static class ParryTuning
    {
        private static float _savedBlockTimer;
        private static bool _scaled;

        public static void ScaleBlockTimer(ref float blockTimer)
        {
            _scaled = false;

            if (!GlobalState.ToggleHacks)
                return;

            float multiplier = GlobalState.Config.GodModeOptions.ParryWindowMultiplier;
            if (multiplier <= 0f || multiplier == 1f)
                return;

            // -1f is the game's "not blocking" sentinel - leave it untouched
            // so the original != -1f check behaves exactly as vanilla.
            if (blockTimer < 0f)
                return;

            _savedBlockTimer = blockTimer;
            blockTimer /= multiplier;
            _scaled = true;
        }

        public static void RestoreBlockTimer(ref float blockTimer)
        {
            if (!_scaled)
                return;

            blockTimer = _savedBlockTimer;
            _scaled = false;
        }
    }
}
