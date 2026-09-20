using System;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Scales processing station capacity (input queue + fuel) by a

    /// configurable multiplier. The Smelter component drives the smelter,

    /// charcoal kiln, blast furnace, windmill, spinning wheel, oven and eitr

    /// refinery; a postfix on Smelter.Awake multiplies m_maxOre / m_maxFuel
    /// once per placed station, so interaction checks, the hover text
    /// ("Windmill (23/500)") and all downstream logic use the boosted values.
    /// </summary>
    internal static class StationCapacity
    {
        private const string Prefix = "StationCapacity";

        private static bool IsEnabled => GlobalState.ToggleHacks && GlobalState.Config.StationsOptions.CapacityMultiplier > 0f;

        public static void Apply(Smelter smelter)
        {
            try
            {
                if (smelter == null || !IsEnabled)
                    return;

                float multiplier = GlobalState.Config.StationsOptions.CapacityMultiplier;
                if (multiplier == 1f)
                    return;

                int ore = smelter.m_maxOre;
                int fuel = smelter.m_maxFuel;
                smelter.m_maxOre = Scale(ore, multiplier);
                smelter.m_maxFuel = Scale(fuel, multiplier);

                HarmonyLog.Log($"[{Prefix}] {smelter.m_name}: x{multiplier} -> ore {smelter.m_maxOre} (was {ore}), fuel {smelter.m_maxFuel} (was {fuel}).");
            }
            catch (Exception ex)
            {
                HarmonyLog.Log($"[{Prefix}] Apply exception: {ex}.");
            }
        }

        private static int Scale(int value, float multiplier)
        {
            // 0 means "this station has no such slot" (e.g. windmill has no
            // fuel) - keep disabled slots disabled.
            if (value <= 0)
                return value;

            return (int)Math.Round(value * multiplier);
        }
    }
}
