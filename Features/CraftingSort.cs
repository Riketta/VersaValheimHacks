using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Sorts the crafting panel list: alphabetically by localized item name,
    /// ties broken by the required crafting-station level - so same-name
    /// upgrade recipes list lowest station level first. Vanilla offers no
    /// purely alphabetical mode (all of its sorts group by craftable state
    /// and hand-placed category weights), so this re-sorts the finished list
    /// right after the game's own UpdateCraftingPanel sorting.
    /// </summary>
    internal static class CraftingSort
    {
        private static readonly FieldInfo AvailableRecipesField =
            AccessTools.Field(typeof(InventoryGui), "m_availableRecipes");

        private static PropertyInfo _recipeProperty;

        public static void SortCraftingPanel(InventoryGui gui)
        {
            if (gui is null || !GlobalState.Config.RecipeOptions.SortCraftingPanel)
                return;

            try
            {
                if (!(AvailableRecipesField?.GetValue(gui) is IList list) || list.Count < 2)
                    return;

                // RecipeDataPair is a private nested struct: read its Recipe
                // property by reflection, sort a shadow list, write the order back.
                if (_recipeProperty is null)
                    _recipeProperty = list[0].GetType().GetProperty("Recipe");

                var shadow = new List<(Recipe Recipe, object Pair)>(list.Count);
                foreach (var pair in list)
                {
                    if (_recipeProperty is null || !(_recipeProperty.GetValue(pair) is Recipe recipe))
                    {
                        HarmonyLog.Log("[CraftingSort] Unexpected list entry shape; leaving list untouched.");
                        return;
                    }

                    shadow.Add((recipe, pair));
                }

                shadow.Sort((a, b) => Compare(a.Recipe, b.Recipe));

                for (int i = 0; i < shadow.Count; i++)
                    list[i] = shadow[i].Pair;
            }
            catch (Exception ex)
            {
                HarmonyLog.Log($"[CraftingSort] Exception: {ex}.");
            }
        }

        private static int Compare(Recipe a, Recipe b)
        {
            int byName = string.Compare(LocalizedName(a), LocalizedName(b), StringComparison.CurrentCulture);
            if (byName != 0)
                return byName;

            int byStation = a.m_minStationLevel.CompareTo(b.m_minStationLevel);
            if (byStation != 0)
                return byStation;

            return string.Compare(a.name, b.name, StringComparison.Ordinal);
        }

        private static string LocalizedName(Recipe recipe)
        {
            string token = recipe.m_item != null ? recipe.m_item.m_itemData.m_shared.m_name : recipe.name;
            Localization localization = Localization.instance;
            return localization != null ? localization.Localize(token) : token;
        }
    }
}
