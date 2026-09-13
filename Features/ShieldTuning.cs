using HarmonyLib;
using System.Reflection;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Reduces damage taken by the player's blocking shield and shows the shield
    /// status as a HUD notification.
    /// </summary>
    internal static class ShieldTuning
    {
        public static readonly FieldInfo CharacterField = AccessTools.Field(typeof(SE_Shield), "m_character");

        public static void CompensateShieldDamage(SE_Shield shield, HitData hit, ref float damage, float totalAbsorbDamage)
        {
            var character = CharacterField.GetValue(shield) as Character;
            if (character is null || !character.IsPlayer())
                return;

            float hitDamage = hit.GetTotalDamage();
            float multiplier = GlobalState.Config.GodModeOptions.ShieldDamageMultiplier;
            if (multiplier >= 0f && multiplier <= 1f)
            {
                float compensated = hitDamage * (1f - multiplier);
                damage -= compensated;
            }

            float remaining = totalAbsorbDamage - damage - hitDamage;
            NotificationManager.Notification($"Shield: {remaining:F0}; Damage: {-hitDamage:F0}.", MessageHud.MessageType.TopLeft);
        }
    }
}
