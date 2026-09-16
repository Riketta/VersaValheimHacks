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
    /// Sorts the crafting panel list by crafting tier (required station
    /// level), then by progression region (inferred from the recipe's
    /// ingredients - the game has no biome tag on items), then alphabetically.
    /// Vanilla offers no such mode, so this re-sorts the finished list right
    /// after the game's own UpdateCraftingPanel sorting.
    ///
    /// Also colors each row's label with its region's color from the fixed
    /// Nature palette; recipes with no known region fall back to gray.
    /// Non-craftable rows keep their color dimmed, preserving vanilla's
    /// craftable/dimmed distinction.
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

        /// <summary>Last-resort palette used when the configured palette is missing or broken.</summary>
        private static readonly string[] FallbackColorsHex =
        {
            "#8BC34A", // Meadows      green
            "#2D5016", // Black Forest dark green
            "#5B5A35", // Swamp        olive brown
            "#CFE8F0", // Mountain     pale blue
            "#D4AF37", // Plains       golden yellow
            "#7C6A8A", // Mistlands    purple
            "#B23A2E", // Ashlands     red-orange
            "#3F7EA6", // Deep North   glacier blue
        };

        /// <summary>Color for rows whose region cannot be inferred.</summary>
        private static readonly Color UnknownRegionColor = new Color(0.55f, 0.55f, 0.55f);

        /// <summary>Unknown-region rows sort after all known regions inside their tier.</summary>
        private const int UnknownRegionSlot = int.MaxValue;

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

                var shadow = new List<(Recipe Recipe, object Pair, GameObject Element, int Region)>(list.Count);
                foreach (var pair in list)
                {
                    if (!(_recipeProperty.GetValue(pair) is Recipe recipe))
                        return;

                    var element = _elementProperty?.GetValue(pair) as GameObject;
                    shadow.Add((recipe, pair, element, ResolveRegion(recipe)));
                }

                shadow.Sort((a, b) => Compare(a.Recipe, a.Region, b.Recipe, b.Region));

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
        /// Colors each crafting row label with its region's color from the
        /// Nature palette; rows with no inferred region stay gray. Non-craftable
        /// rows keep their color dimmed.
        /// </summary>
        public static void ColorizeByRegion(InventoryGui gui)
        {
            if (gui is null || !GlobalState.Config.RecipeOptions.ColorByRegion)
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

                    int region = ResolveRegion(recipe);
                    Color color = region >= 0 ? ResolveRegionColor(region) : UnknownRegionColor;

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
        /// Progression region of a recipe: the highest home region among its
        /// known ingredients, or -1 when none are mapped.
        /// </summary>
        private static int ResolveRegion(Recipe recipe)
        {
            int best = -1;
            var resources = recipe.m_resources;
            if (resources is null)
                return best;

            foreach (var requirement in resources)
            {
                if (requirement?.m_resItem is null)
                    continue;

                if (ItemDatabase.TryGetRegion(requirement.m_resItem.name, out int region) && region > best)
                    best = region;
            }

            return best;
        }

        /// <summary>
        /// Resolves a region's color from the palette selected in
        /// ColorPalette (looked up case-insensitively in ColorPalettes;
        /// unknown/broken palettes fall back to "Nature", then to the
        /// built-in fallback colors). Parsed per call so config reloads
        /// apply live.
        /// </summary>
        private static Color ResolveRegionColor(int region)
        {
            var options = GlobalState.Config.RecipeOptions;
            List<string> palette = GetPalette(options.ColorPalette)
                ?? GetPalette("Nature");

            if (palette is null || palette.Count == 0)
                palette = new List<string>(FallbackColorsHex);

            int index = Mathf.Clamp(region, 0, palette.Count - 1);
            string hex = (palette[index] ?? string.Empty).Trim();
            if (!hex.StartsWith("#"))
                hex = "#" + hex;

            if (ColorUtility.TryParseHtmlString(hex, out Color color))
                return color;

            string fallback = FallbackColorsHex[Mathf.Clamp(region, 0, FallbackColorsHex.Length - 1)];
            ColorUtility.TryParseHtmlString(fallback, out color);
            return color;
        }

        private static List<string> GetPalette(string name)
        {
            var palettes = GlobalState.Config.RecipeOptions.ColorPalettes;
            if (palettes is null || string.IsNullOrEmpty(name))
                return null;

            foreach (var kv in palettes)
                if (string.Equals(kv.Key, name, StringComparison.OrdinalIgnoreCase))
                    return kv.Value;

            return null;
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

        private static int Compare(Recipe a, int regionA, Recipe b, int regionB)
        {
            // Crafting tier first (lowest bench first), then region
            // (unknown-region rows last), then alphabetically.
            int byStation = a.m_minStationLevel.CompareTo(b.m_minStationLevel);
            if (byStation != 0)
                return byStation;

            int slotA = regionA >= 0 ? regionA : UnknownRegionSlot;
            int slotB = regionB >= 0 ? regionB : UnknownRegionSlot;
            int byRegion = slotA.CompareTo(slotB);
            if (byRegion != 0)
                return byRegion;

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
