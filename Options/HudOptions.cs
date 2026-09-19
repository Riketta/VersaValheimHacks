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


        /// <summary>
        /// Vertical offset in pixels for the energy bars (stamina, eitr,
        /// adrenaline): the same shift is applied to every bar, so they move
        /// together and keep their relative arrangement. 0 = vanilla
        /// position, positive = up. The health bar block is not touched.
        /// </summary>
        public float EnergyBarsVerticalOffset { get; set; } = 0f;
    }
}
