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

        /// <summary>
        /// Items marked with the stack-protection hotkey (Numpad6 over an
        /// item slot) are skipped by every stack-to-chest operation. The
        /// mark is stored in the item's vanilla custom data, so it persists
        /// across sessions and syncs in multiplayer.
        /// </summary>
        public bool ProtectMarkedItems { get; set; } = true;

        /// <summary>
        /// Outline color drawn on marked item slots ("#RRGGBB").
        /// </summary>
        public string MarkedColor { get; set; } = "#FF5A5A";
    }
}
