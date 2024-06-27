using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Skills;

namespace VersaValheimHacks.Patches
{
    internal class StatusEffectsTuning
    {
        //[HarmonyPatch(typeof(SE_Shield), "Setup")]
        internal class SE_ShieldSetup
        {
            private const string Prefix = "SE_Shield.Setup";

            [HarmonyPrefix]
            public static void Debug(SE_Shield __instance, Character character, float ___m_totalAbsorbDamage, float ___m_damage)
            {
                HarmonyLog.Log($"[{Prefix}.Postfix] Character: {character.name} ({character.m_name}); Max Shield: {___m_totalAbsorbDamage}; Damage: {___m_damage}.");
            }
        }

        //[HarmonyPatch(typeof(SE_Shield), "SetLevel")]
        internal class SE_ShieldSetLevel
        {
            private const string Prefix = "SE_Shield.SetLevel";

            [HarmonyPrefix]
            public static void Debug(SE_Shield __instance, int itemLevel, float skillLevel, float ___m_totalAbsorbDamage, float ___m_damage)
            {
                HarmonyLog.Log($"[{Prefix}.Postfix] ItemLevel: {itemLevel}; Skill: {skillLevel}; Max Shield: {___m_totalAbsorbDamage}; Damage: {___m_damage}.");
            }
        }

        [HarmonyPatch(typeof(SE_Shield), "OnDamaged")]
        internal class SE_ShieldOnDamaged
        {
            private const string Prefix = "SE_Shield.OnDamaged";

            public static FieldInfo m_characterField = AccessTools.Field(typeof(SE_Shield), "m_character");

            [HarmonyPrefix]
            public static void Debug(SE_Shield __instance, HitData hit, float ___m_totalAbsorbDamage, ref float ___m_damage) // (global::HitData hit, global::Character attacker)
            {
                HarmonyLog.Log($"[{Prefix}.Prefix] Shield: {___m_totalAbsorbDamage}; Damage: {___m_damage}.");

                var character = m_characterField.GetValue(__instance) as Character;
                //HarmonyLog.Log($"[{Prefix}.Prefix] Unit: {character.m_name}; IsPlayer: {character.IsPlayer()}.");
                //bool isObjectPlayer = ValheimUtils.IsObjectPlayer(character.gameObject);
                //HarmonyLog.Log($"[{Prefix}.Prefix] Unit: {character.m_name}; IsObjectPlayer: {character.IsPlayer()}.");

                if (!character.IsPlayer()) // || !isObjectPlayer)
                    return;

                float hitDamage = hit.GetTotalDamage();
                if (GlobalState.Config.GodModeOptions.ShieldDamageMultiplier >= 0f && GlobalState.Config.GodModeOptions.ShieldDamageMultiplier <= 1f)
                {
                    float modifiedDamage = hitDamage * (1f - GlobalState.Config.GodModeOptions.ShieldDamageMultiplier);
                    //HarmonyLog.Log($"[{Prefix}.Prefix] Compensating shield damage: m_damage += {hitDamage} - {modifiedDamage} ({(modifiedDamage / hitDamage) * 100:F2}%).");

                    ___m_damage -= modifiedDamage;
                }

                float currentShieldValue = ___m_totalAbsorbDamage - ___m_damage - hitDamage;
                NotificationManager.Notification($"Shield: {currentShieldValue:F0}; Damage: {-hitDamage:F0}.", MessageHud.MessageType.TopLeft);
            }
        }
    }
}
