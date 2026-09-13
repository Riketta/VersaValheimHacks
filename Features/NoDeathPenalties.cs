using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Keep skills and food buffs after death.
    /// </summary>
    internal static class NoDeathPenalties
    {
        public static readonly FieldInfo FoodsField = AccessTools.Field(typeof(Player), "m_foods");

        private static readonly List<Player.Food> _foodBackup = new List<Player.Food>(3);

        /// <summary>Patch prefix returns the inverse: original skill reset is always skipped.</summary>
        public static bool SkipSkillReset => true;

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
            NotificationManager.Notification("Death penalties skipped: skills & food preserved.", MessageHud.MessageType.TopLeft);
        }
    }
}
