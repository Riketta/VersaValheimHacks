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



        private static bool MasterEnabled => GlobalState.ToggleHacks && GlobalState.Config.BetterPowersOptions.Enabled;



        // Streamer mode keeps the vanilla cooldown: instant re-casts are a visible tell.
        private static bool CooldownEnabled => !GlobalState.Config.StreamerMode && MasterEnabled;

        // Extra powers stay available in streamer mode while their icons are
        // hidden from the HUD - otherwise they would be visible and break the
        // vanilla look.
        private static bool ExtraPowersEnabled => MasterEnabled && (!GlobalState.Config.StreamerMode || GlobalState.Config.BetterPowersOptions.HideExtraPowerVisuals);

        /// <summary>Power TTL override value; 0 when the category gate is off (vanilla duration).</summary>

        private static float EffectiveDuration =>

            GlobalState.Config.BuffsOptions.OverridePower

                ? GlobalState.Config.BetterPowersOptions.Duration

                : 0f;



        /// <summary>Prefix: hide the cooldown from the game for the duration of the original call.</summary>

        public static void SuppressCooldown(ref float guardianPowerCooldown)

        {

            if (!CooldownEnabled || !GlobalState.Config.BetterPowersOptions.NoCooldown)

                return;



            _savedCooldown = guardianPowerCooldown;

            guardianPowerCooldown = 0f;

        }



        /// <summary>Postfix: restore the real cooldown after the original call.</summary>

        public static void RestoreCooldown(ref float guardianPowerCooldown)

        {

            if (!CooldownEnabled || !GlobalState.Config.BetterPowersOptions.NoCooldown)

                return;



            guardianPowerCooldown = _savedCooldown;

        }



        public static void ApplyExtraPowers(Player player, StatusEffect guardianPower)

        {

            if (!ExtraPowersEnabled || !GlobalState.Config.BetterPowersOptions.StackAllBossPowers)

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
                float duration = EffectiveDuration;
                string durationText = duration > 0f ? $"{duration / 3600f:0.#}h" : "vanilla duration";
                NotificationManager.Notification($"Extra powers ({durationText}): {names}.", MessageHud.MessageType.TopLeft);
            }
        }

        /// <summary>



        /// True when this status effect is one of the stacked extra powers



        /// whose visual feedback is hidden (icons, start messages).



        /// Deliberately independent of god mode (master toggle) and streamer

        /// mode: while the option is enabled, still-running extra buffs stay

        /// hidden even after the master toggle goes off.

        /// </summary>



        public static bool IsHiddenExtraPower(StatusEffect effect)



        {



            if (effect == null || !GlobalState.Config.BetterPowersOptions.StackAllBossPowers || !GlobalState.Config.BetterPowersOptions.HideExtraPowerVisuals)


                return false;

            // SEMan hands out clones, so strip the "(Clone)" suffix.
            string prefabName = Utils.GetPrefabName(effect.name);
            if (!GlobalState.Config.BetterPowersOptions.BuffExtraPowers.TryGetValue(prefabName, out bool enabled) || !enabled)

                return false;



            string selected = Player.m_localPlayer != null ? Player.m_localPlayer.GetGuardianPowerName() : null;

            return !string.Equals(prefabName, selected, StringComparison.Ordinal);

        }

        /// <summary>
        /// Postfix on SEMan.GetHUDStatusEffects: drops the stacked extra
        /// powers from the HUD list so their icons disappear. Purely visual

        /// - the status effects stay active - and the icon of the power the
        /// player deliberately selected at the trophy stand remains.
        /// </summary>
        public static void HideExtraPowerVisuals(List<StatusEffect> effects)
        {
            if (effects is null || effects.Count == 0)
                return;

            for (int i = effects.Count - 1; i >= 0; i--)
            {
                if (IsHiddenExtraPower(effects[i]))

                    effects.RemoveAt(i);

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
                float duration = EffectiveDuration;
                if (duration > 0f)
                    power.m_ttl = duration; // maximum buff duration; disabled override keeps the vanilla power TTL
                TimeField.SetValue(power, 0f);                                 // currently elapsed buff time
            }
            catch (Exception e)
            {
                HarmonyLog.Log($"[BetterPowers] Exception: {e}.");
            }
        }
    }
}
