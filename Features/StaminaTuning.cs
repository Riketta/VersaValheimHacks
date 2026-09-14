using System;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Scales the regen delay that is applied every time stamina is spent.
    /// </summary>
    internal static class StaminaTuning
    {
        public static void ScaleRegenDelay(ref float regenTimer)
        {
            if (!GlobalState.ToggleHacks)
                return;

            regenTimer *= Math.Max(0f, GlobalState.Config.StaminaOptions.RegenDelayMultiplier);
        }
    }
}
