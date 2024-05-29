using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            [HarmonyPostfix]
            public static void ApplyAllBuffs(Player __instance, StatusEffect ___m_guardianSE)
            {
                if (!GlobalState.EnableHacks)
                    return;

                HarmonyLog.Log($"Current Guardian: \"{___m_guardianSE.name}\" ({___m_guardianSE.NameHash()}).");

                // GP_Eikthyr.
                // GP_TheElder.
                // GP_Bonemass.
                // GP_Moder.
                // GP_Yagluth.
                // GP_Queen
                // GP_Ashlands.
                // GP_DeepNorth.

                __instance.GetSEMan().AddStatusEffect("GP_Bonemass".GetStableHashCode(), resetTime: true);
                __instance.GetSEMan().AddStatusEffect("GP_Eikthyr".GetStableHashCode(), resetTime: true);
            }
        }
    }
}
