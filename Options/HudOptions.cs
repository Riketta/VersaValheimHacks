namespace VersaValheimHacks.Options
{
    internal class HudOptions
    {
        /// <summary>
        /// Show a countdown under the HUD health value: seconds until the
        /// next food healing tick.
        /// </summary>
        public bool HealthRegenCountdown { get; set; } = true;

        /// <summary>
        /// Show the shield durability bar next to the HP bar while a shield
        /// status effect is active.
        /// </summary>
        public bool ShieldDurabilityBar { get; set; } = true;
    }
}
