using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Sorts the crafting panel list into bench-tier blocks: grouped by the
    /// required crafting-station level (lowest first) and alphabetically by
    /// localized name inside each block. Vanilla offers no such mode (all of
    /// its sorts group by craftable state and hand-placed category weights),
    /// so this re-sorts the finished list right after the game's own
    /// UpdateCraftingPanel sorting.
    /// </summary>
    internal static class CraftingSort
    {
        private static readonly FieldInfo AvailableRecipesField =
            AccessTools.Field(typeof(InventoryGui), "m_availableRecipes");

        private static readonly FieldInfo RecipeListSpaceField =
            AccessTools.Field(typeof(InventoryGui), "m_recipeListSpace");

        private static PropertyInfo _recipeProperty;
        private static PropertyInfo _elementProperty;

        public static void SortCraftingPanel(InventoryGui gui)
        {
            if (gui is null || !GlobalState.Config.RecipeOptions.SortCraftingPanel)
                return;

            try
            {
                if (!(AvailableRecipesField?.GetValue(gui) is IList list) || list.Count < 2)
                    return;

                // RecipeDataPair is a private nested struct: read its Recipe
                // and InterfaceElement properties by reflection, sort a shadow
                // list, write the order and row positions back.
                if (_recipeProperty is null || _elementProperty is null)
                {
                    var pairType = list[0].GetType();
                    _recipeProperty = pairType.GetProperty("Recipe");
                    _elementProperty = pairType.GetProperty("InterfaceElement");
                }

                var shadow = new List<(Recipe Recipe, object Pair, GameObject Element)>(list.Count);
                foreach (var pair in list)
                {
                    if (_recipeProperty is null || !(_recipeProperty.GetValue(pair) is Recipe recipe))
                    {
                        HarmonyLog.Log("[CraftingSort] Unexpected list entry shape; leaving list untouched.");
                        return;
                    }

                    var element = _elementProperty?.GetValue(pair) as GameObject;
                    shadow.Add((recipe, pair, element));
                }

                shadow.Sort((a, b) => Compare(a.Recipe, b.Recipe));

                // Reorder the data list AND move the row elements to their new
                // index positions - vanilla positioned them before this postfix
                // ran, so without this the on-screen order would never change.
                float space = RecipeListSpaceField != null ? Convert.ToSingle(RecipeListSpaceField.GetValue(gui)) : 0f;
                for (int i = 0; i < shadow.Count; i++)
                {
                    list[i] = shadow[i].Pair;

                    if (shadow[i].Element != null && shadow[i].Element.transform is RectTransform rect)
                        rect.anchoredPosition = new Vector2(0f, i * -space);
                }
            }
            catch (Exception ex)
            {
                HarmonyLog.Log($"[CraftingSort] Exception: {ex}.");
            }
        }

        private static int Compare(Recipe a, Recipe b)
        {
            // Station level first (tier blocks, lowest bench first),
            // alphabetical inside each block.
            int byStation = a.m_minStationLevel.CompareTo(b.m_minStationLevel);
            if (byStation != 0)
                return byStation;

            int byName = string.Compare(LocalizedName(a), LocalizedName(b), StringComparison.CurrentCulture);
            if (byName != 0)
                return byName;

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
