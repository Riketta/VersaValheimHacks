using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Reduces damage taken by the player's blocking shield and shows the shield
    /// status as a HUD notification.
    /// </summary>
    internal static class ShieldTuning
    {
        public static readonly FieldInfo CharacterField = AccessTools.Field(typeof(SE_Shield), "m_character");
        public static readonly FieldInfo TotalAbsorbField = AccessTools.Field(typeof(SE_Shield), "m_totalAbsorbDamage");
        public static readonly FieldInfo DamageField = AccessTools.Field(typeof(SE_Shield), "m_damage");

        private const float NotificationInterval = 2f;
        private static float _lastNotificationTime = -999f;

        // The instantiated start-effect GameObjects (the bubble) are tracked
        // privately by the base class.
        private static readonly FieldInfo StartEffectInstancesField =
            AccessTools.Field(typeof(StatusEffect), "m_startEffectInstances");

        private static readonly string[] BubbleColorProperties = { "_TintColor", "_Color", "_EmissionColor" };

        /// <summary>
        /// Single entry point for the SE_Shield.Setup/SetLevel postfixes.
        /// </summary>
        public static void OnShieldApplied(SE_Shield shield)
        {
            ResetDurability(shield);
            RecolorBubble(shield);
        }

        /// <summary>
        /// Tints the shield bubble VFX toward the configured hex color. Only
        /// affects the currently spawned effect instances (per cast), never
        /// the shared prefab assets, and is skipped in streamer mode.
        /// </summary>
        public static void RecolorBubble(SE_Shield shield)
        {
            try
            {
                string hex = GlobalState.Config.GodModeOptions.ShieldBubbleColorHex;
                if (string.IsNullOrWhiteSpace(hex))
                    return;

                if (GlobalState.Config.StreamerMode)
                    return;

                if (!ColorUtility.TryParseHtmlString(hex.Trim(), out Color target))
                {
                    HarmonyLog.Log($"[ShieldTuning] ShieldBubbleColorHex '{hex}' is not a valid color - skipped.");
                    return;
                }

                if (StartEffectInstancesField?.GetValue(shield) is not GameObject[] instances || instances.Length == 0)
                    return;

                var block = new MaterialPropertyBlock();
                int tinted = 0;
                foreach (GameObject instance in instances)
                {
                    if (instance == null)
                        continue;

                    foreach (ParticleSystem particles in instance.GetComponentsInChildren<ParticleSystem>(true))
                    {
                        var main = particles.main;
                        main.startColor = new ParticleSystem.MinMaxGradient(target);
                        tinted++;
                    }

                    foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
                    {
                        renderer.GetPropertyBlock(block);
                        foreach (string property in BubbleColorProperties)
                            block.SetColor(property, target);
                        renderer.SetPropertyBlock(block);
                        tinted++;
                    }
                }

                HarmonyLog.Log($"[ShieldTuning] Bubble tinted {hex} ({tinted} nodes).");
            }
            catch (Exception ex)
            {
                HarmonyLog.Log($"[ShieldTuning] RecolorBubble exception: {ex}.");
            }
        }

        /// <summary>
        /// The game never resets accumulated shield damage on re-apply (SetLevel
        /// recomputes only the max), and a fresh effect cloned from the old one can
        /// even inherit it. Called from the Setup/SetLevel postfixes, so every
        /// application or recast hands out a fully repaired shield.
        /// </summary>
        public static void ResetDurability(SE_Shield shield)
        {
            if (!GlobalState.Config.GodModeOptions.RefreshDurabilityOnCast)
                return;

            float max = (float)TotalAbsorbField.GetValue(shield);
            DamageField.SetValue(shield, 0f);
            HarmonyLog.Log($"[ShieldTuning] Shield applied: durability restored to {max:0}.");
        }

        private static SE_Shield _warnedShield;
        private static float _warnedAtTime;

        /// <summary>
        /// Runs every frame for every SE_Shield instance: when the shield is
        /// about to end by timeout, shows one center-screen warning (same
        /// style as power activation). The same SE instance survives recasts
        /// with its timer reset, so the warning re-arms whenever the elapsed
        /// time drops back below the point it fired at.
        /// </summary>
        public static void CheckExpiryWarning(SE_Shield shield)
        {
            try
            {
                float warnSeconds = GlobalState.Config.GodModeOptions.ShieldExpiryWarningSeconds;
                if (warnSeconds <= 0 || GlobalState.Config.StreamerMode)
                    return;

                Character character = CharacterField.GetValue(shield) as Character;
                if (character == null || !character.IsPlayer() || character != Player.m_localPlayer)
                    return;

                float elapsed = shield.GetDuration();
                if (shield == _warnedShield && elapsed >= _warnedAtTime)
                    return; // already warned for this cast window

                float remaining = shield.GetRemaningTime();
                if (shield.m_ttl <= 0 || remaining <= 0 || remaining > warnSeconds)
                    return;

                _warnedShield = shield;
                _warnedAtTime = elapsed;

                if (MessageHud.instance != null)
                    MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, $"Magic shield expires in {warnSeconds:0} seconds.");
                HarmonyLog.Log($"[ShieldTuning] Shield expiry warning: {remaining:0.#} s remaining.");
            }
            catch (Exception ex)
            {
                HarmonyLog.Log($"[ShieldTuning] CheckExpiryWarning exception: {ex}.");
            }
        }

        public static void CompensateShieldDamage(SE_Shield shield, HitData hit, ref float damage, float totalAbsorbDamage)
        {
            var character = CharacterField.GetValue(shield) as Character;
            if (character is null || !character.IsPlayer())
                return;

            float hitDamage = hit.GetTotalDamage();
            float multiplier = GlobalState.Config.GodModeOptions.ShieldDamageMultiplier;
            if (GlobalState.Config.StreamerMode)
                multiplier = Mathf.Max(multiplier, 0.5f); // streamer mode: the shield visibly takes real damage

            if (multiplier >= 0f && multiplier <= 1f)
            {
                float compensated = hitDamage * (1f - multiplier);
                damage -= compensated;
            }

            // One status line per hit is spam in long fights; report at most every few seconds.
            if (Time.time - _lastNotificationTime < NotificationInterval)
                return;

            _lastNotificationTime = Time.time;
            float remaining = totalAbsorbDamage - damage - hitDamage;
            NotificationManager.Notification($"Shield: {remaining:F0}; Damage: {-hitDamage:F0}.", MessageHud.MessageType.TopLeft);
        }
    }
}
