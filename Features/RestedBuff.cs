using System;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Extended rested buff duration (base + per comfort level).
    /// </summary>
    internal static class RestedBuff
    {
        /// <summary>Gate for the vanilla auto-reset: re-resting re-arms rested to full duration.</summary>
        public static bool AllowAutoRefresh => GlobalState.Config.BuffsOptions.RestAutoRefresh;
        // Streamer mode keeps the vanilla rested duration. A zero config value
        // disables that override and keeps the vanilla value as well, and so
        // does the OverrideRest gate.
        public static void ApplyDurationOverrides(SE_Rested rested)
        {
            if (GlobalState.Config.StreamerMode || !GlobalState.Config.BuffsOptions.OverrideRest)
                return;

            float baseTtl = GlobalState.Config.BuffsOptions.RestDurationBase;
            float perComfort = GlobalState.Config.BuffsOptions.RestDurationPerComfort;
            if (baseTtl > 0f)
                rested.m_baseTTL = baseTtl;
            if (perComfort > 0f)
                rested.m_TTLPerComfortLevel = perComfort;

            HarmonyLog.Log($"[RestedBuff] Base TTL {rested.m_baseTTL}, per comfort {rested.m_TTLPerComfortLevel}.");
        }

        /// <summary>
        /// Hotkey handler: apply the rested status effect on demand with the
        /// base rest duration (BuffsOptions.RestDurationBase; 0 = vanilla base
        /// 300 s). Comfort levels are not added. SE_Rested computes its
        /// comfort TTL only in Setup (during AddStatusEffect) and ResetTime,
        /// so the m_ttl override below sticks. Purely client-side.
        /// </summary>
        public static void ApplyToPlayer()
        {
            if (GlobalState.Player is null)
            {
                HarmonyLog.Log("[RestedBuff] Can't apply rested: no player instance saved!");
                return;
            }

            float configured = GlobalState.Config.BuffsOptions.RestDurationBase;
            float duration = configured > 0f ? configured : VanillaBaseDuration;

            try
            {
                var seman = GlobalState.Player.GetSEMan();
                var rested = seman.AddStatusEffect("Rested".GetStableHashCode(), resetTime: true);
                if (rested is null)
                {
                    HarmonyLog.Log("[RestedBuff] 'Rested' status effect not found in ObjectDB.");
                    return;
                }

                rested.m_ttl = duration;

                NotificationManager.Notification($"Rested applied ({duration:0} s).", MessageHud.MessageType.TopLeft);
                HarmonyLog.Log($"[RestedBuff] Applied rested for {duration:0} s (configured base {configured:0}).");
            }
            catch (Exception ex)
            {
                HarmonyLog.Log($"[RestedBuff] Exception: {ex}.");
            }
        }

        /// <summary>Vanilla SE_Rested.m_baseTTL.</summary>
        public const float VanillaBaseDuration = 300f;
    }
}
