using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Re-eating the same food, extended food duration and stronger health regen.
    /// When food cycling is enabled, an extended food restarts on its natural burn
    /// time when the extended timer runs out, instead of disappearing.
    /// </summary>
    internal static class BetterEating
    {
        public static readonly FieldInfo FoodsField = AccessTools.Field(typeof(Player), "m_foods");

        /// <summary>Foods currently on the extended (mod) duration.</summary>
        private static readonly HashSet<Player.Food> _extended = new HashSet<Player.Food>();

        private static bool FeatureEnabled => GlobalState.ToggleHacks && GlobalState.Config.BetterEatingOptions.Enabled;

        private static bool CyclingEnabled => GlobalState.Config.BetterEatingOptions.Enabled && GlobalState.Config.BetterEatingOptions.FoodCycling;

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

            // Streamer mode: keep natural food timers; cycling refreshes them invisibly.
            if (GlobalState.Config.StreamerMode)
            {
                _extended.Clear();
                return;
            }

            var foods = FoodsField.GetValue(player) as List<Player.Food>;
            HarmonyLog.Log($"[BetterEating] Resetting {foods.Count} food timer(s) to {GlobalState.Config.BetterEatingOptions.FoodBuffDuration}s.");
            foreach (var food in foods)
                food.m_time = GlobalState.Config.BetterEatingOptions.FoodBuffDuration;

            TrackExtended(foods);
        }

        /// <summary>
        /// Prefix of Player.UpdateFood: a food about to expire restarts on its natural
        /// burn time instead of being removed. Extended foods do this when the extended
        /// timer runs out; in streamer mode every food cycles so buffs never break while
        /// the HUD always shows vanilla-looking timers. Converting before the original
        /// tick keeps the food in the list, so vanilla never clamps the player's current
        /// stats down.
        /// </summary>
        public static void CycleExpiredFood(Player player)
        {
            if (!CyclingEnabled && !GlobalState.Config.StreamerMode)
                return;

            var foods = FoodsField.GetValue(player) as List<Player.Food>;
            foreach (var food in foods)
            {
                if (food.m_time <= 1f && (_extended.Remove(food) || GlobalState.Config.StreamerMode))
                {
                    HarmonyLog.Log($"[BetterEating] Cycling food: {food.m_name} -> natural {food.m_item.m_shared.m_foodBurnTime}s.");
                    food.m_time = food.m_item.m_shared.m_foodBurnTime;
                }
            }
        }

        /// <summary>Hotkey handler: end the extended duration of all foods now.</summary>
        public static void CycleNow()
        {
            if (!CyclingEnabled)
                return;

            if (GlobalState.Player is null)
            {
                HarmonyLog.Log("[BetterEating] Can't cycle food: no player instance saved!");
                return;
            }

            var foods = FoodsField.GetValue(GlobalState.Player) as List<Player.Food>;
            int count = 0;
            foreach (var food in foods)
            {
                if (_extended.Contains(food))
                {
                    _extended.Remove(food);
                    food.m_time = food.m_item.m_shared.m_foodBurnTime;
                    count++;
                }
            }

            NotificationManager.Notification(count > 0
                ? $"Cycled {count} food(s) to natural duration."
                : "No extended food to cycle.", MessageHud.MessageType.TopLeft);
        }

        /// <summary>Hotkey handler: remove all currently eaten food buffs.</summary>
        public static void ClearFoodNow()
        {
            if (GlobalState.Player is null)
            {
                HarmonyLog.Log("[BetterEating] Can't clear food: no player instance saved!");
                return;
            }

            var foods = FoodsField.GetValue(GlobalState.Player) as List<Player.Food>;
            int count = foods.Count;

            GlobalState.Player.ClearFood();
            _extended.Clear();

            NotificationManager.Notification(count > 0
                ? $"Cleared {count} food buff(s)."
                : "No food to clear.", MessageHud.MessageType.TopLeft);
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

        private static void TrackExtended(List<Player.Food> foods)
        {
            _extended.Clear();
            foreach (var food in foods)
                _extended.Add(food);
        }
    }
}
