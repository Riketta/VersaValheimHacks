using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
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
    ///
    /// Optionally also colors each row's label by its required station level
    /// (up to 8 tiers); recipes the station cannot craft keep the tier color
    /// but dimmed, preserving vanilla's craftable/dimmed distinction.
    /// </summary>
    internal static class CraftingSort
    {
        private static readonly FieldInfo AvailableRecipesField =
            AccessTools.Field(typeof(InventoryGui), "m_availableRecipes");

        private static readonly FieldInfo RecipeListSpaceField =
            AccessTools.Field(typeof(InventoryGui), "m_recipeListSpace");

        private static PropertyInfo _recipeProperty;
        private static PropertyInfo _elementProperty;
        private static PropertyInfo _canCraftProperty;

        /// <summary>Row label colors per required station level (1..8, 8+ = last).</summary>
        private static readonly Color[] TierColors =
        {
            new Color(0.55f, 0.55f, 0.55f), // 1 gray
            new Color(1.00f, 1.00f, 1.00f), // 2 white
            new Color(0.35f, 1.00f, 0.35f), // 3 green
            new Color(0.35f, 0.65f, 1.00f), // 4 blue
            new Color(0.75f, 0.40f, 1.00f), // 5 purple
            new Color(1.00f, 0.62f, 0.20f), // 6 orange
            new Color(1.00f, 0.30f, 0.30f), // 7 red
            new Color(0.20f, 1.00f, 1.00f), // 8+ cyan
        };

        /// <summary>RGB dim factor for recipes the station cannot craft.</summary>
        private const float NotCraftableDim = 0.45f;

        public static void SortCraftingPanel(InventoryGui gui)
        {
            if (gui is null || !GlobalState.Config.RecipeOptions.SortCraftingPanel)
                return;

            try
            {
                if (!(AvailableRecipesField?.GetValue(gui) is IList list) || list.Count < 2)
                    return;

                if (!EnsurePairProperties(list))
                {
                    HarmonyLog.Log("[CraftingSort] Unexpected list entry shape; leaving list untouched.");
                    return;
                }

                var shadow = new List<(Recipe Recipe, object Pair, GameObject Element)>(list.Count);
                foreach (var pair in list)
                {
                    if (!(_recipeProperty.GetValue(pair) is Recipe recipe))
                        return;

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

        /// <summary>
        /// Colors each crafting row label by its required station level
        /// (1 gray, 2 white, 3 green, 4 blue, 5 purple, 6 orange, 7 red,
        /// 8+ cyan). Non-craftable rows keep the tier color dimmed, so the
        /// vanilla craftable/dimmed distinction is preserved.
        /// </summary>
        public static void ColorizeByTier(InventoryGui gui)
        {
            if (gui is null || !GlobalState.Config.RecipeOptions.ColorizeByTier)
                return;

            try
            {
                if (!(AvailableRecipesField?.GetValue(gui) is IList list) || list.Count == 0)
                    return;

                if (!EnsurePairProperties(list))
                    return;

                foreach (var pair in list)
                {
                    if (!(_recipeProperty.GetValue(pair) is Recipe recipe))
                        continue;

                    var element = _elementProperty?.GetValue(pair) as GameObject;
                    var nameTransform = element != null ? element.transform.Find("name") : null;
                    var label = nameTransform != null ? nameTransform.GetComponent<TMP_Text>() : null;
                    if (label is null)
                        continue;

                    int tier = Mathf.Clamp(recipe.m_minStationLevel, 1, TierColors.Length);
                    Color color = TierColors[tier - 1];

                    bool canCraft = _canCraftProperty is null || (_canCraftProperty.GetValue(pair) as bool? ?? true);
                    if (!canCraft)
                        color = new Color(color.r * NotCraftableDim, color.g * NotCraftableDim, color.b * NotCraftableDim, 1f);

                    label.color = color;
                }
            }
            catch (Exception ex)
            {
                HarmonyLog.Log($"[CraftingSort] Colorize exception: {ex}.");
            }
        }

        private static bool EnsurePairProperties(IList list)
        {
            if (_recipeProperty != null && _elementProperty != null && _canCraftProperty != null)
                return true;

            var pairType = list[0].GetType();
            _recipeProperty = pairType.GetProperty("Recipe");
            _elementProperty = pairType.GetProperty("InterfaceElement");
            _canCraftProperty = pairType.GetProperty("CanCraft");
            return _recipeProperty != null;
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
