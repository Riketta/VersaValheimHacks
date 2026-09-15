namespace VersaValheimHacks.Options
{
    internal class AreaStackOptions
    {
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Radius in meters around the player to search for containers
        /// (chests, carts, ...) when the area-stack hotkey is pressed.
        /// </summary>
        public float Radius { get; set; } = 15f;
    }
}
