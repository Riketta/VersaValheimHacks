using HarmonyLib;
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
