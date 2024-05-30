using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VersaValheimHacks.Patches
{
    internal class FreeCrafting
    {
        [HarmonyPatch(typeof(Player), "NoCostCheat")]
        internal class NoCostCheat
        {
            private const string Prefix = "Player.NoCostCheat";
            public static bool Status = true;

            //[HarmonyPrefix]
            public static bool EnableNoCostCheat(Player __instance, ref RefResult<bool> __resultRef)
            {
                HarmonyLog.Log($"[{Prefix}.Prefix] Trying to replace return value.");
                __resultRef = () => ref Status;

                return false;
            }

            [HarmonyPostfix]
            public static void EnableNoCostCheat(ref bool __result)
            {
                if (!GlobalState.ToggleHacks)
                    return;

                //HarmonyLog.Log($"[{Prefix}.Postfix] Trying to update return value.");
                __result = Status;
            }
        }

        [HarmonyPatch(typeof(ZoneSystem), "GetGlobalKey", new Type[] { typeof(GlobalKeys) })]
        internal class GetGlobalKey
        {
            private const string Prefix = "ZoneSystem.GetGlobalKey";
            public static bool Status = true;

            [HarmonyPostfix]
            public static void GetFakeGlobalKey(GlobalKeys key, ref bool __result)
            {
                if (!GlobalState.ToggleHacks)
                    return;

                if (key == GlobalKeys.NoCraftCost)
                {
                    //HarmonyLog.Log($"[{Prefix}.Postfix] Trying to update return value.");
                    __result = Status;
                }
                //else
                //    HarmonyLog.Log($"[{Prefix}.Postfix] Skipping wrong case: {key}.");
            }
        }
    }
}
