using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Player;

namespace VersaValheimHacks.Patches
{
    internal class NoDeathPenalties
    {
        [HarmonyPatch(typeof(Skills), "Clear")]
        internal class SkillsClear
        {
            private const string Prefix = "Skills.Clear";

            [HarmonyPrefix]
            public static bool PreventSkillProgressLoss()
            {
                HarmonyLog.Log($"[{Prefix}.Prefix] Preventing skill progress loss on death.");
                return false;
            }
        }

        [HarmonyPatch(typeof(Skills), "OnDeath")]
        internal class SkillsOnDeath
        {
            private const string Prefix = "Skills.OnDeath";

            [HarmonyPrefix]
            public static bool PreventSkillProgressLoss()
            {
                HarmonyLog.Log($"[{Prefix}.Prefix] Preventing skill progress loss on death.");
                return false;
            }
        }

        [HarmonyPatch(typeof(Player), "OnDeath")]
        internal class PlayerOnDeath
        {
            private const string Prefix = "Player.OnDeath";

            // private readonly System.Collections.Generic.List<global::Player.Food> global::Player.m_foods
            public static FieldInfo m_foodsField = AccessTools.Field(typeof(Player), "m_foods");
            public static List<Food> FoodBuffsBackup = new List<Food>(3);

            [HarmonyPrefix]
            public static void BackupFoodBuffs(Player __instance)
            {
                if (__instance is null)
                    return;

                HarmonyLog.Log($"[{Prefix}.Prefix] Saving food buffs...");

                List<Food> m_foods = m_foodsField.GetValue(__instance) as List<Food>;

                FoodBuffsBackup.Clear();
                foreach (var food in m_foods)
                {
                    HarmonyLog.Log($"[{Prefix}.Prefix] Saving buff: {food.m_name}.");
                    FoodBuffsBackup.Add(food);
                }
            }

            [HarmonyPostfix]
            public static void RestoreFoodBuffsFromBackup(Player __instance)
            {
                HarmonyLog.Log($"[{Prefix}.Postfix] Trying to restore food buffs...");

                List<Food> m_foods = m_foodsField.GetValue(__instance) as List<Food>;
                foreach (var food in FoodBuffsBackup)
                {
                    HarmonyLog.Log($"[{Prefix}.Prefix] Trying to restore buff: {food.m_name}.");
                    m_foods.Add(food);
                }
            }
        }
    }
}
