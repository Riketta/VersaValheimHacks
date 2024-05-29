﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static CharacterAnimEvent;
using static Player;

namespace VersaValheimHacks.Patches
{
    internal class BetterEating
    {
        [HarmonyPatch(typeof(Player.Food), "CanEatAgain")]
        internal class CanEatAgain
        {
            private const string Prefix = "Player.Food.CanEatAgain";

            [HarmonyPostfix]
            public static void AllowToEatSameFood(Player.Food __instance, ref bool __result)
            {
                if (!GlobalState.EnableHacks)
                    return;

                //HarmonyLog.Log($"[{Prefix}.Postfix] Reeating: {__instance.m_name}.");
                __result = true;
            }
        }

        [HarmonyPatch(typeof(Player), "EatFood")]
        internal class EatFood
        {
            private const string Prefix = "Player.EatFood";

            public static FieldInfo m_foodsField = AccessTools.Field(typeof(Player), "m_foods");

            [HarmonyPostfix]
            public static void IncreaseFoodTimer(Player __instance, ItemDrop.ItemData item)
            {
                List<Food> m_foods = m_foodsField.GetValue(__instance) as List<Food>;
                foreach (var food in m_foods)
                {
                    HarmonyLog.Log($"[{Prefix}.Postfix] Updating timer: {food.m_name} (current time: {food.m_time}).");
                    food.m_time = 24 * 60 * 60;
                }
            }
        }
    }
}
