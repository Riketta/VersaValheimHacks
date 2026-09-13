namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Removes mist particle effects (e.g. Mistlands mist).
    /// </summary>
    internal static class NoMist
    {
        /// <summary>Patch prefix returns this: false skips the mist update entirely.</summary>
        public static bool AllowUpdate => !(GlobalState.ToggleHacks && GlobalState.Config.GodModeOptions.DisableMistlandsMist);
    }
}
