namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Tracks the local player instance and crouch state (populated by the
    /// SetCrouch patch; other features rely on it).
    /// </summary>
    internal static class PlayerState
    {
        public static void Update(Player player, bool crouching)
        {
            if (player != null)
                GlobalState.Player = player;

            GlobalState.IsPlayerCrouching = crouching;

            if (crouching)
                DebugTools.OnCrouching();
        }

        /// <summary>
        /// Called every frame at the main menu: the game scene is unloaded, so
        /// the cached player reference would point at a destroyed object.
        /// </summary>
        public static void ClearPlayer()
        {
            GlobalState.Player = null;
            GlobalState.IsPlayerCrouching = false;
        }
    }
}
