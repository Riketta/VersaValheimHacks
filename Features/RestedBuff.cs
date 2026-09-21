using HarmonyLib;
using System;
using System.Reflection;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Applies the rested buff on demand with the game's real current rest
    /// value (vanilla formula, computed by SE_Rested.UpdateTTL from the
    /// shelter/comfort pieces around you: base 300 s + 60 s per comfort
    /// level). Also gates the optional duration override used by the
    /// auto-refresh feature.
    /// </summary>
    internal static class RestedBuff
    {
        private const string Prefix = "RestedBuff";

        private static readonly FieldInfo TimeField = AccessTools.Field(typeof(StatusEffect), "m_time");

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

            HarmonyLog.Log($"[{Prefix}] Base TTL {rested.m_baseTTL}, per comfort {rested.m_TTLPerComfortLevel}.");
        }

        /// <summary>
        /// Hotkey handler: apply (or refresh) the rested status effect with
        /// the real current rest value. No TTL override here - SE_Rested
        /// computes it in Setup / ResetTime from the actual comfort level
        /// (base 300 s + 60 s per level); re-applying keeps a higher
        /// remaining time, exactly like vanilla re-resting. Purely
        /// client-side.
        /// </summary>
        public static void ApplyToPlayer()
        {
            if (GlobalState.Player is null)
            {
                HarmonyLog.Log($"[{Prefix}] Can't apply rested: no player instance saved!");
                return;
            }

            try
            {
                StatusEffect rested = GlobalState.Player.GetSEMan().AddStatusEffect("Rested".GetStableHashCode(), resetTime: true);
                if (rested is null)
                {
                    HarmonyLog.Log($"[{Prefix}] 'Rested' status effect not found in ObjectDB.");
                    return;
                }

                float elapsed = TimeField?.GetValue(rested) as float? ?? 0f;
                float remaining = rested.m_ttl - elapsed;
                int comfort = GlobalState.Player.GetComfortLevel();

                NotificationManager.Notification($"Rested applied ({remaining:0} s, comfort {comfort}).", MessageHud.MessageType.TopLeft);
                HarmonyLog.Log($"[{Prefix}] Applied rested: {remaining:0} s remaining, comfort {comfort}.");
            }
            catch (Exception ex)
            {
                HarmonyLog.Log($"[{Prefix}] Exception: {ex}.");
            }
        }
    }
}
