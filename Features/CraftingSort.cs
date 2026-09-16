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

        /// <summary>Row label colors per required station level, as hex strings (tier 1..8, 8+ = last entry).</summary>
        private static readonly string[] DefaultTierColorsHex =
        {
            "#8C8C8C", // 1 gray
            "#FFFFFF", // 2 white
            "#59FF59", // 3 green
            "#59A6FF", // 4 blue
            "#BF66FF", // 5 purple
            "#FF9E33", // 6 orange
            "#FF4D4D", // 7 red
            "#33FFFF", // 8+ cyan
        };

        /// <summary>"Nature" layout: muted earth tones (tier 1..8, 8+ = last entry).</summary>
        private static readonly string[] NatureTierColorsHex =
        {
            "#8BC34A", // 1 green
            "#2D5016", // 2 dark green
            "#5B5A35", // 3 olive brown
            "#CFE8F0", // 4 pale blue
            "#D4AF37", // 5 golden yellow
            "#7C6A8A", // 6 purple
            "#B23A2E", // 7 red-orange
            "#3F7EA6", // 8+ glacier blue
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

                    int tier = Mathf.Max(recipe.m_minStationLevel, 1);
                    Color color = ResolveTierColor(tier);

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

        /// <summary>
        /// Resolves a tier's color from the configured layout and hex list
        /// (parse errors fall back to that layout's built-in palette). Tiers
        /// beyond the list length share the last entry; parsed per call so
        /// config reloads apply live.
        /// </summary>
        private static Color ResolveTierColor(int tier)
        {
            bool nature = string.Equals(GlobalState.Config.RecipeOptions.TierColorsLayout, "Nature", StringComparison.OrdinalIgnoreCase);
            string[] builtIn = nature ? NatureTierColorsHex : DefaultTierColorsHex;

            var hexList = nature ? null : GlobalState.Config.RecipeOptions.TierColorsHex;
            if (hexList is null || hexList.Count == 0)
                hexList = new List<string>(builtIn);

            int index = Mathf.Clamp(tier - 1, 0, hexList.Count - 1);
            string hex = (hexList[index] ?? string.Empty).Trim();
            if (!hex.StartsWith("#"))
                hex = "#" + hex;

            if (ColorUtility.TryParseHtmlString(hex, out Color color))
                return color;

            string fallback = builtIn[Mathf.Clamp(tier - 1, 0, builtIn.Length - 1)];
            ColorUtility.TryParseHtmlString(fallback, out color);
            return color;
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
