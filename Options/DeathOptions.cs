namespace VersaValheimHacks.Options
{
    internal class DeathOptions
    {
        /// <summary>
        /// Restore eaten food after death (vanilla clears the stomach).
        /// Remaining food timers are preserved.
        /// </summary>
        public bool RestoreFoodOnDeath { get; set; } = true;

        /// <summary>
        /// Restore beneficial status effects after death (rested, guardian
        /// power, demister, cozy, positive attribute buffs) with their
        /// remaining timers. Debuffs are always left to vanilla's wipe.
        /// </summary>
        public bool RestoreBuffsOnDeath { get; set; } = true;
    }
}
