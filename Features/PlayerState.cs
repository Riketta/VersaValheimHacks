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
    }
}
