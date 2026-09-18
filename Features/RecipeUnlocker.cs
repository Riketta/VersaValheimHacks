using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Unlocks every enabled crafting recipe. Two entry points: automatically
    /// once per session (opt-in: debug mode AND RecipeOptions.UnlockAllDebug),
    /// or on demand via HotkeysOptions.UnlockAllRecipes (debug mode required).
    /// Recipe knowledge is client-side only (Player.m_knownRecipes) - the
    /// server is never told about it.
    /// </summary>
    internal static class RecipeUnlocker
    {
        private const string Prefix = "RecipeUnlocker";

        public static readonly MethodInfo AddKnownRecipeMethod = AccessTools.Method(typeof(Player), "AddKnownRecipe");

        private static readonly FieldInfo KnownRecipesField = AccessTools.Field(typeof(Player), "m_knownRecipes");
        private static readonly FieldInfo InventoryAnimatorField = AccessTools.Field(typeof(InventoryGui), "m_animator");
        private static readonly MethodInfo UpdateCraftingPanelMethod = AccessTools.Method(typeof(InventoryGui), "UpdateCraftingPanel");

        private static bool _unlocked;

        /// <summary>Patch prefix returns false after unlocking to skip the original method.</summary>
        public static bool UnlockAll(Player player)
        {
            if (!GlobalState.ToggleExtraHacks || !GlobalState.Config.RecipeOptions.UnlockAllDebug || player is null || ObjectDB.instance is null || _unlocked)
                return true;

            HarmonyLog.Log("[RecipeUnlocker] Unlocking all enabled recipes.");
            var addKnownRecipe = AccessTools.MethodDelegate<Action<Recipe>>(AddKnownRecipeMethod, player);

            foreach (Recipe recipe in ObjectDB.instance.m_recipes)
                if (recipe.m_enabled)
                    addKnownRecipe(recipe);

            _unlocked = true;
            return false;
        }

        /// <summary>
        /// Unlocks every enabled recipe on demand. Re-presses are harmless:
        /// the game's AddKnownRecipe dedupes and stays silent for known recipes.
        /// </summary>
        public static void UnlockAllOnDemand()
        {
            try
            {
                if (!GlobalState.ToggleExtraHacks)
                {
                    NotificationManager.Notification("Recipe unlock requires debug mode (toggle with Num *).", MessageHud.MessageType.TopLeft);
                    return;
                }

                Player player = GlobalState.Player;
                if (player == null || ObjectDB.instance == null || AddKnownRecipeMethod == null)
                {
                    NotificationManager.Notification("Recipe unlock unavailable (enter a world first).", MessageHud.MessageType.TopLeft);
                    return;
                }

                int knownBefore = CountKnownRecipes(player);
                var addKnownRecipe = AccessTools.MethodDelegate<Action<Recipe>>(AddKnownRecipeMethod, player);

                foreach (Recipe recipe in ObjectDB.instance.m_recipes)
                    if (recipe.m_enabled)
                        addKnownRecipe(recipe);

                int knownAfter = CountKnownRecipes(player);
                RefreshCraftingPanel();

                HarmonyLog.Log($"[{Prefix}] On-demand unlock: {knownAfter - knownBefore} new, {knownAfter} known in total.");
                NotificationManager.Notification(
                    knownAfter > knownBefore
                        ? $"All recipes unlocked ({knownAfter - knownBefore} new, {knownAfter} total)."
                        : "All recipes already unlocked.",
                    MessageHud.MessageType.TopLeft);
            }
            catch (Exception ex)
            {
                HarmonyLog.Log($"[{Prefix}] UnlockAllOnDemand exception: {ex}.");
                NotificationManager.Notification("Recipe unlock failed (see log).", MessageHud.MessageType.TopLeft);
            }
        }

        private static int CountKnownRecipes(Player player)
        {
            return KnownRecipesField?.GetValue(player) is HashSet<string> known ? known.Count : 0;
        }

        /// <summary>Rebuilds the crafting list so new recipes appear without closing the panel.</summary>
        private static void RefreshCraftingPanel()
        {
            try
            {
                if (InventoryGui.instance == null || UpdateCraftingPanelMethod == null)
                    return;

                if (!(InventoryAnimatorField?.GetValue(InventoryGui.instance) is Animator animator) || animator == null || !animator.GetBool("visible"))
                    return;

                UpdateCraftingPanelMethod.Invoke(InventoryGui.instance, new object[] { false });
            }
            catch (Exception ex)
            {
                HarmonyLog.Log($"[{Prefix}] RefreshCraftingPanel exception: {ex}.");
            }
        }
    }
}
