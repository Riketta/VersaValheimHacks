namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Extended rested buff duration (base + per comfort level).
    /// </summary>
    internal static class RestedBuff
    {
        // Streamer mode keeps the vanilla rested duration. A zero config value
        // disables that override and keeps the vanilla value as well.
        public static void ApplyDurationOverrides(SE_Rested rested)
        {
            if (GlobalState.Config.StreamerMode)
                return;

            float baseTtl = GlobalState.Config.BuffsOptions.RestDurationBase;
            float perComfort = GlobalState.Config.BuffsOptions.RestDurationPerComfort;
            if (baseTtl > 0f)
                rested.m_baseTTL = baseTtl;
            if (perComfort > 0f)
                rested.m_TTLPerComfortLevel = perComfort;

            HarmonyLog.Log($"[RestedBuff] Base TTL {rested.m_baseTTL}, per comfort {rested.m_TTLPerComfortLevel}.");
        }
    }
}
