namespace VersaValheimHacks.Options
{
    internal class StaminaOptions
    {
        /// <summary>
        /// Multiplier for the delay before stamina regen resumes after
        /// spending stamina (0 = instant regen, 1 = vanilla 1 s).
        /// </summary>
        public float RegenDelayMultiplier { get; set; } = 0.25f;
    }
}
