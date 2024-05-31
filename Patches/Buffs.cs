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
    internal class Buffs
    {
        [HarmonyPatch(typeof(SE_Rested), "Setup")]
        internal class RestedDuration
        {
            private const string Prefix = "SE_Rested.Setup";

            public static FieldInfo m_timeField = AccessTools.Field(typeof(StatusEffect), "m_time");

            [HarmonyPrefix]
            public static void ModifyTTLInSetup(SE_Rested __instance)
            {
                float m_time = (float)m_timeField.GetValue(__instance);
                HarmonyLog.Log($"[{Prefix}.Prefix | Premodified] Duration: {m_time}; TTL: {__instance.m_ttl}; Base TTL: {__instance.m_baseTTL}; TTL per Comfort: {__instance.m_TTLPerComfortLevel}.");

                HarmonyLog.Log($"[{Prefix}.Prefix] Increasing base Rested duration to {GlobalState.Config.BuffsOptions.RestDurationBase} seconds.");
                __instance.m_baseTTL = GlobalState.Config.BuffsOptions.RestDurationBase;

                HarmonyLog.Log($"[{Prefix}.Prefix] Increasing Rested duration per comfort level to {GlobalState.Config.BuffsOptions.RestDurationPerComfort} seconds.");
                __instance.m_baseTTL = GlobalState.Config.BuffsOptions.RestDurationPerComfort;
            }
        }

        //[HarmonyPatch(typeof(SE_Rested), "UpdateTTL")]
        internal class Rested
        {
            private const string Prefix = "SE_Rested.UpdateTTL";

            public static FieldInfo m_timeField = AccessTools.Field(typeof(StatusEffect), "m_time");

            [HarmonyPostfix]
            public static void DebugTTL(SE_Rested __instance)
            {
                float m_time = (float)m_timeField.GetValue(__instance);
                HarmonyLog.Log($"[{Prefix}.Postfix | Rested] Duration: {m_time}; TTL: {__instance.m_ttl}; Base TTL: {__instance.m_baseTTL}; TTL per Comfort: {__instance.m_TTLPerComfortLevel}.");
            }
        }
    }
}
