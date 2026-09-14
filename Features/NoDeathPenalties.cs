using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Soften death penalties: scale the death skill drain down and keep food buffs.
    /// </summary>
    internal static class NoDeathPenalties
    {
        public static readonly FieldInfo FoodsField = AccessTools.Field(typeof(Player), "m_foods");

        private static readonly List<Player.Food> _foodBackup = new List<Player.Food>(3);

        public static void ScaleDeathDrain(ref float factor)
        {
            if (!GlobalState.ToggleHacks)
                return;

            factor *= Math.Max(0f, GlobalState.Config.SkillsOptions.DeathDrainMultiplier);
        }

        public static void BackupFoods(Player player)
        {
            if (player is null)
                return;

            var foods = FoodsField.GetValue(player) as List<Player.Food>;
            HarmonyLog.Log($"[NoDeathPenalties] Backing up {foods.Count} food buff(s).");

            _foodBackup.Clear();
            _foodBackup.AddRange(foods);
        }

        public static void RestoreFoods(Player player)
        {
            var foods = FoodsField.GetValue(player) as List<Player.Food>;
            HarmonyLog.Log($"[NoDeathPenalties] Restoring {_foodBackup.Count} food buff(s).");

            foreach (var food in _foodBackup)
                foods.Add(food);

            _foodBackup.Clear();
            NotificationManager.Notification($"Death penalties: food preserved, {DescribeDrain()}.", MessageHud.MessageType.TopLeft);
        }

        private static string DescribeDrain()
        {
            if (!GlobalState.ToggleHacks)
                return "skill drain normal";

            float multiplier = Math.Max(0f, GlobalState.Config.SkillsOptions.DeathDrainMultiplier);
            return multiplier == 0f ? "no skill drain" : $"skill drain x{multiplier:0.##}";
        }
    }
}
