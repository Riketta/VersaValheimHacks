using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Guardian powers without cooldown, optionally stacking extra boss powers.
    /// </summary>
    internal static class BetterPowers
    {
        public static readonly FieldInfo TimeField = AccessTools.Field(typeof(StatusEffect), "m_time");

        private static float _savedCooldown;

        // Streamer mode keeps vanilla cooldowns and skips extra buff icons.
        private static bool FeatureEnabled => !GlobalState.Config.StreamerMode && GlobalState.ToggleHacks && GlobalState.Config.BetterPowersOptions.Enabled;

        /// <summary>Prefix: hide the cooldown from the game for the duration of the original call.</summary>
        public static void SuppressCooldown(ref float guardianPowerCooldown)
        {
            if (!FeatureEnabled)
                return;

            _savedCooldown = guardianPowerCooldown;
            guardianPowerCooldown = 0f;
        }

        /// <summary>Postfix: restore the real cooldown after the original call.</summary>
        public static void RestoreCooldown(ref float guardianPowerCooldown)
        {
            if (!FeatureEnabled)
                return;

            guardianPowerCooldown = _savedCooldown;
        }

        public static void ApplyExtraPowers(Player player, StatusEffect guardianPower)
        {
            if (!FeatureEnabled || !GlobalState.Config.BetterPowersOptions.ApplyAllBuffs)
                return;

            HarmonyLog.Log($"[BetterPowers] Current guardian: \"{guardianPower.name}\" ({guardianPower.NameHash()}).");

            var enabled = new List<string>();
            foreach (var pair in GlobalState.Config.BetterPowersOptions.BuffExtraPowers)
            {
                if (!pair.Value)
                    continue;

                enabled.Add(pair.Key);
                Activate(player, pair.Key);
            }

            if (enabled.Count > 0)
            {
                string names = string.Join(", ", enabled);
                float duration = GlobalState.Config.BetterPowersOptions.Duration;
                string durationText = duration > 0f ? $"{duration / 3600f:0.#}h" : "vanilla duration";
                NotificationManager.Notification($"Extra powers ({durationText}): {names}.", MessageHud.MessageType.TopLeft);
            }
        }

        private static void Activate(Player player, string powerName)
        {
            try
            {
                int powerHash = powerName.GetStableHashCode();
                player.GetSEMan().AddStatusEffect(powerHash, resetTime: true);
                StatusEffect power = player.GetSEMan().GetStatusEffect(powerHash);

                if (power is null)
                {
                    HarmonyLog.Log($"[BetterPowers] No \"{powerName}\" power!");
                    return;
                }

                HarmonyLog.Log($"[BetterPowers] Power \"{powerName}\": TTL {power.m_ttl}, time {(float)TimeField.GetValue(power)}.");
                float duration = GlobalState.Config.BetterPowersOptions.Duration;
                if (duration > 0f)
                    power.m_ttl = duration; // maximum buff duration; 0 keeps the vanilla power TTL
                TimeField.SetValue(power, 0f);                                 // currently elapsed buff time
            }
            catch (Exception e)
            {
                HarmonyLog.Log($"[BetterPowers] Exception: {e}.");
            }
        }
    }
}
