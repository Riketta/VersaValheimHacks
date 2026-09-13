using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Re-eating the same food, extended food duration and stronger health regen.
    /// </summary>
    internal static class BetterEating
    {
        public static readonly FieldInfo FoodsField = AccessTools.Field(typeof(Player), "m_foods");

        private static bool FeatureEnabled => GlobalState.ToggleHacks && GlobalState.Config.BetterEatingOptions.Enabled;

        public static void AllowReEating(ref bool canEatAgain)
        {
            if (!FeatureEnabled)
                return;

            canEatAgain = true;
        }

        public static void ResetFoodTimers(Player player)
        {
            if (!GlobalState.Config.BetterEatingOptions.Enabled)
                return;

            var foods = FoodsField.GetValue(player) as List<Player.Food>;
            foreach (var food in foods)
            {
                HarmonyLog.Log($"[BetterEating] Resetting food timer: {food.m_name} (was {food.m_time}).");
                food.m_time = GlobalState.Config.BetterEatingOptions.FoodBuffDuration;
            }
        }

        public static void ScaleHealthRegen(ref float regenMultiplier)
        {
            var options = GlobalState.Config.BetterEatingOptions;
            if (!options.Enabled || options.HealingMultiplier < 0.001f)
                return;

            regenMultiplier = regenMultiplier < 0.001f
                ? options.HealingMultiplier
                : regenMultiplier * options.HealingMultiplier;
        }

        /// <summary>Hotkey handler: reset remaining time of the currently eaten food.</summary>
        public static void RefreshActiveFood()
        {
            if (GlobalState.Player is null)
            {
                HarmonyLog.Log("[BetterEating] Can't refresh food: no player instance saved!");
                return;
            }

            foreach (var food in GlobalState.Player.GetFoods())
            {
                HarmonyLog.Log($"[BetterEating] Refreshing food: {food.m_name} = {GlobalState.Config.BetterEatingOptions.FoodBuffDuration} (was {food.m_time}).");
                food.m_time = GlobalState.Config.BetterEatingOptions.FoodBuffDuration;
            }
        }
    }
}
