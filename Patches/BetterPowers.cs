using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VersaValheimHacks.Patches
{
    internal class BetterPowers
    {
        [HarmonyPatch(typeof(Player), "StartGuardianPower")]
        internal class StartGuardianPower
        {
            private const string Prefix = "Player.StartGuardianPower";

            public static float m_guardianPowerCooldown = 0f;

            [HarmonyPrefix]
            public static void NoPowerCooldown(Player __instance, ref float ___m_guardianPowerCooldown)
            {
                if (!GlobalState.EnableHacks)
                    return;

                m_guardianPowerCooldown = ___m_guardianPowerCooldown;
                ___m_guardianPowerCooldown = 0f;
            }

            [HarmonyPostfix]
            public static void RevertPowerCooldown(Player __instance, ref float ___m_guardianPowerCooldown)
            {
                if (!GlobalState.EnableHacks)
                    return;

                ___m_guardianPowerCooldown = m_guardianPowerCooldown;
            }
        }

        [HarmonyPatch(typeof(Player), "ActivateGuardianPower")]
        internal class ActivateGuardianPower
        {
            private const string Prefix = "Player.ActivateGuardianPower";

            public static float m_guardianPowerCooldown = 0f;

            [HarmonyPrefix]
            public static void NoPowerCooldown(Player __instance, ref float ___m_guardianPowerCooldown)
            {
                if (!GlobalState.EnableHacks)
                    return;

                m_guardianPowerCooldown = ___m_guardianPowerCooldown;
                ___m_guardianPowerCooldown = 0f;
            }

            [HarmonyPostfix]
            public static void RevertPowerCooldown(Player __instance, ref float ___m_guardianPowerCooldown)
            {
                if (!GlobalState.EnableHacks)
                    return;

                ___m_guardianPowerCooldown = m_guardianPowerCooldown;
            }

            public static FieldInfo m_timeField = AccessTools.Field(typeof(StatusEffect), "m_time");

            [HarmonyPostfix]
            public static void ApplyAllBuffs(Player __instance, StatusEffect ___m_guardianSE)
            {
                if (!GlobalState.EnableHacks)
                    return;

                HarmonyLog.Log($"[{Prefix}] Current Guardian: \"{___m_guardianSE.name}\" ({___m_guardianSE.NameHash()}).");

                // GP_Eikthyr.
                // GP_TheElder.
                // GP_Bonemass.
                // GP_Moder.
                // GP_Yagluth.
                // GP_Queen
                // GP_Ashlands.
                // GP_DeepNorth.

                ActivatePower(__instance, "GP_Bonemass");
                ActivatePower(__instance, "GP_Eikthyr");
            }

            public static void ActivatePower(Player player, string powerName)
            {
                try
                {
                    int powerHash = powerName.GetStableHashCode();

                    player.GetSEMan().AddStatusEffect(powerHash, resetTime: true);
                    StatusEffect power = player.GetSEMan().GetStatusEffect(powerHash);

                    if (power is null)
                        HarmonyLog.Log($"[{Prefix}] No \"{powerName}\" power!");
                    else
                    {
                        float m_time = (float)m_timeField.GetValue(power);
                        HarmonyLog.Log($"[{Prefix} | Power] TTL: {power.m_ttl}; Time: {m_time}.");

                        power.m_ttl *= 50f;
                        m_timeField.SetValue(power, m_time * 50f);
                    }
                }
                catch (Exception e)
                {
                    HarmonyLog.Log($"[{Prefix}] Exception: {e}.");
                }
            }
        }
    }
}
