namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Extended rested buff duration (base + per comfort level).
    /// </summary>
    internal static class RestedBuff
    {
        public static void ApplyDurationOverrides(SE_Rested rested)
        {
            rested.m_baseTTL = GlobalState.Config.BuffsOptions.RestDurationBase;
            rested.m_TTLPerComfortLevel = GlobalState.Config.BuffsOptions.RestDurationPerComfort;

            HarmonyLog.Log($"[RestedBuff] Base TTL {rested.m_baseTTL}, per comfort {rested.m_TTLPerComfortLevel}.");
        }
    }
}
