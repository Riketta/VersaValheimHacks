namespace VersaValheimHacks.Options
{
    internal class StationsOptions
    {
        /// <summary>

        /// Multiplier for processing station capacity (input queue and fuel):

        /// smelter, charcoal kiln, blast furnace, windmill, spinning wheel,

        /// oven and eitr refinery. 10 = e.g. windmill 50 -> 500, smelter

        /// 10 -> 100. 1 = vanilla. Applied when a station spawns, so
        /// already-placed stations pick it up on world load.

        /// </summary>
        public float CapacityMultiplier { get; set; } = 10f;
    }
}
