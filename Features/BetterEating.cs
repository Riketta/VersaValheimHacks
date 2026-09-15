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
    /// Also a persistent food loadout: save the currently eaten foods to the
    /// config and re-apply that exact set later, at natural values.
    /// </summary>
    internal static class BetterEating
    {
        public static readonly FieldInfo FoodsField = AccessTools.Field(typeof(Player), "m_foods");

        /// <summary>Foods currently on the extended (mod) duration.</summary>
        private static readonly HashSet<Player.Food> _extended = new HashSet<Player.Food>();

        private static bool FeatureEnabled => GlobalState.ToggleHacks && GlobalState.Config.BetterEatingOptions.Enabled;

        private static bool CyclingEnabled => GlobalState.Config.BetterEatingOptions.Enabled && GlobalState.Config.BetterEatingOptions.FoodCycling;

        /// <summary>Override off (BuffsOptions.OverrideFood) or FoodBuffDuration = 0: foods keep vanilla burn times.</summary>
        private static bool FoodOverrideDisabled =>
            !GlobalState.Config.BuffsOptions.OverrideFood ||
            GlobalState.Config.BetterEatingOptions.FoodBuffDuration <= 0f;

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

            // Duration override disabled (0): foods keep their natural burn
            // times; cycling still restarts them at natural end.
            if (FoodOverrideDisabled)
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
            // FoodAutoReset = false disables the auto-restart entirely: food is
            // removed (vanilla) when its timer ends, extended or not.
            if (!GlobalState.Config.BuffsOptions.FoodAutoReset)
                return;

            // Streamer mode cycles every food invisibly; with the duration
            // override disabled (FoodBuffDuration = 0) cycling keeps
            // vanilla-timer food alive on its natural burn time.
            bool cycleAll = GlobalState.Config.StreamerMode || (FoodOverrideDisabled && CyclingEnabled);
            if (!CyclingEnabled && !cycleAll)
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

        /// <summary>
        /// Hotkey handler: record the currently eaten foods to the config as a
        /// persistent loadout. Saving with an empty stomach is a no-op that
        /// keeps the previously saved set.
        /// </summary>
        public static void SaveCurrentFood()
        {
            if (GlobalState.Player is null)
            {
                HarmonyLog.Log("[BetterEating] Can't save food: no player instance saved!");
                return;
            }

            var foods = FoodsField.GetValue(GlobalState.Player) as List<Player.Food>;

            // Guard: an empty stomach must never wipe the saved loadout.
            if (foods is null || foods.Count == 0)
            {
                NotificationManager.Notification("No food to save — kept the existing saved set.", MessageHud.MessageType.TopLeft);
                return;
            }

            var options = GlobalState.Config.BetterEatingOptions;
            options.SavedFood.Clear();
            foreach (var food in foods)
            {
                if (!string.IsNullOrEmpty(food.m_name))
                    options.SavedFood.Add(food.m_name);
            }

            GlobalState.Config.Save();
            NotificationManager.Notification($"Saved {options.SavedFood.Count} food(s) to config.", MessageHud.MessageType.TopLeft);
        }

        /// <summary>
        /// Hotkey handler: replace the currently eaten foods with the saved
        /// loadout, created exactly like vanilla eating - natural burn times
        /// and natural stats, no mod modifications.
        /// </summary>
        public static void ApplySavedFood()
        {
            if (GlobalState.Player is null || ObjectDB.instance is null)
            {
                HarmonyLog.Log("[BetterEating] Can't apply saved food: no player or ObjectDB yet!");
                return;
            }

            var options = GlobalState.Config.BetterEatingOptions;
            if (options.SavedFood.Count == 0)
            {
                NotificationManager.Notification("No saved food in config — save a set with Numpad2 first.", MessageHud.MessageType.TopLeft);
                return;
            }

            var foods = FoodsField.GetValue(GlobalState.Player) as List<Player.Food>;
            foods.Clear();
            _extended.Clear(); // applied food is not "extended"; cycling must not touch it

            int applied = 0;
            foreach (string prefabName in options.SavedFood)
            {
                var prefab = ObjectDB.instance.GetItemPrefab(prefabName);
                var itemDrop = prefab != null ? prefab.GetComponent<ItemDrop>() : null;
                if (itemDrop is null)
                {
                    HarmonyLog.Log($"[BetterEating] Saved food not found in ObjectDB: {prefabName}.");
                    continue;
                }

                var item = itemDrop.m_itemData;
                foods.Add(new Player.Food
                {
                    m_name = prefabName,
                    m_item = item,
                    m_time = item.m_shared.m_foodBurnTime,
                    m_health = item.m_shared.m_food,
                    m_stamina = item.m_shared.m_foodStamina,
                    m_eitr = item.m_shared.m_foodEitr,
                });
                applied++;
            }

            NotificationManager.Notification(applied > 0
                ? $"Applied saved food set ({applied} food(s))."
                : "No saved food could be applied.", MessageHud.MessageType.TopLeft);
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
