using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
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
                HarmonyLog.Log($"[{Prefix}.Postfix] Character: {character.name} ({character.m_name}); Shield: {___m_totalAbsorbDamage - ___m_damage}.");
                NotificationManager.Notification($"Character: {character.name} ({character.m_name}); Shield: {___m_totalAbsorbDamage - ___m_damage}.", MessageHud.MessageType.TopLeft);
            }
        }

        //[HarmonyPatch(typeof(SE_Shield), "SetLevel")]
        internal class SE_ShieldSetLevel
        {
            private const string Prefix = "SE_Shield.SetLevel";

            [HarmonyPrefix]
            public static void Debug(SE_Shield __instance, int itemLevel, float skillLevel, float ___m_totalAbsorbDamage, float ___m_damage)
            {
                HarmonyLog.Log($"[{Prefix}.Postfix] ItemLevel: {itemLevel}; Skill: {skillLevel}; Shield: {___m_totalAbsorbDamage - ___m_damage}.");
                NotificationManager.Notification($"ItemLevel: {itemLevel}; Skill: {skillLevel}; Shield: {___m_totalAbsorbDamage - ___m_damage}.", MessageHud.MessageType.TopLeft);
            }
        }

        //[HarmonyPatch(typeof(SE_Shield), "OnDamaged")]
        internal class SE_ShieldOnDamaged
        {
            private const string Prefix = "SE_Shield.OnDamaged";

            [HarmonyPrefix]
            public static void Debug(SE_Shield __instance, float ___m_totalAbsorbDamage, float ___m_damage) // (global::HitData hit, global::Character attacker)
            {
                HarmonyLog.Log($"[{Prefix}.Postfix] Shield: {___m_totalAbsorbDamage - ___m_damage}.");
                NotificationManager.Notification($"Shield: {___m_totalAbsorbDamage - ___m_damage}.", MessageHud.MessageType.TopLeft);
            }
        }
    }
}
