namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Higher carry weight, no encumbrance.
    /// </summary>
    internal static class NeverEncumbered
    {
        private static bool FeatureEnabled => GlobalState.ToggleHacks && GlobalState.Config.GodModeOptions.NeverEncumbered;

        public static void ForceNotEncumbered(ref bool encumbered)
        {
            if (FeatureEnabled)
                encumbered = false;
        }

        public static void ScaleMaxCarryWeight(ref float maxCarryWeight)
        {
            if (FeatureEnabled)
                maxCarryWeight *= GlobalState.Config.GodModeOptions.CarryWeightMultiplier;
        }
    }
}
