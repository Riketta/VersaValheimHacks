using System;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Scales the regen delay that is applied every time stamina is spent.
    /// </summary>
    internal static class StaminaTuning
    {
        private static float _savedSneakDrain;

        public static void ScaleRegenDelay(ref float regenTimer)
        {
            if (!GlobalState.ToggleHacks)
                return;

            regenTimer *= Math.Max(0f, GlobalState.Config.StaminaOptions.RegenDelayMultiplier);
        }

        /// <summary>Prefix: shrink the sneak drain for the duration of the original call.</summary>
        public static void SuppressSneakDrain(ref float sneakDrain)
        {
            if (!GlobalState.ToggleHacks)
                return;

            _savedSneakDrain = sneakDrain;
            sneakDrain *= Math.Max(0f, GlobalState.Config.StaminaOptions.SneakDrainMultiplier);
        }

        /// <summary>Postfix: restore the real drain after the original call.</summary>
        public static void RestoreSneakDrain(ref float sneakDrain)
        {
            if (!GlobalState.ToggleHacks)
                return;

            sneakDrain = _savedSneakDrain;
        }
    }
}
