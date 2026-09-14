namespace VersaValheimHacks.Options
{
    internal class StaminaOptions
    {
        /// <summary>
        /// Multiplier for the delay before stamina regen resumes after
        /// spending stamina (0 = instant regen, 1 = vanilla 1 s).
        /// </summary>
        public float RegenDelayMultiplier { get; set; } = 0.25f;

        /// <summary>
        /// Multiplier for the stamina drain while sneak-moving
        /// (1 = vanilla 5/s, 0 = free sneaking).
        /// </summary>
        public float SneakDrainMultiplier { get; set; } = 0.25f;
    }
}
