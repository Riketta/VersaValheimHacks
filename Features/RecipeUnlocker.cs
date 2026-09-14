using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Unlocks every enabled crafting recipe once per session.
    /// Opt-in: requires debug mode AND RecipeOptions.UnlockAllDebug.
    /// </summary>
    internal static class RecipeUnlocker
    {
        public static readonly MethodInfo AddKnownRecipeMethod = AccessTools.Method(typeof(Player), "AddKnownRecipe");

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
    }
}
