namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Extended rested buff duration (base + per comfort level).
    /// </summary>
    internal static class RestedBuff
    {
        public static void ApplyDurationOverrides(SE_Rested rested)
        {
            // Streamer mode keeps the vanilla rested duration.
            if (GlobalState.Config.StreamerMode)
                return;

            rested.m_baseTTL = GlobalState.Config.BuffsOptions.RestDurationBase;
            rested.m_TTLPerComfortLevel = GlobalState.Config.BuffsOptions.RestDurationPerComfort;

            HarmonyLog.Log($"[RestedBuff] Base TTL {rested.m_baseTTL}, per comfort {rested.m_TTLPerComfortLevel}.");
        }
    }
}
