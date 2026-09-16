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

        /// <summary>Built-in region colors (index = region): Meadows, Black Forest, Swamp, Mountain, Plains, Mistlands, Ashlands, Deep North.</summary>
        private static readonly string[] BuiltInBrightColorsHex =
        {
            "#8C8C8C", // Meadows      gray
            "#FFFFFF", // Black Forest white
            "#59FF59", // Swamp        green
            "#59A6FF", // Mountain     blue
            "#BF66FF", // Plains       purple
            "#FF9E33", // Mistlands    orange
            "#FF4D4D", // Ashlands     red
            "#33FFFF", // Deep North   cyan
        };

        private static readonly string[] BuiltInNatureColorsHex =
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

        /// <summary>
        /// Raw material prefab name -> home region index. Deliberately only
        /// raw/biome-specific sources: processed or biome-neutral goods (coal,
        /// bronze nails...) inherit their recipe's other ingredients. Unknown
        /// ingredients are ignored; a recipe with no known ingredient falls
        /// back to the gray "unknown" color and sorts last in its tier.
        /// </summary>
        private static readonly Dictionary<string, int> IngredientRegion =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            // Meadows
            ["Wood"] = 0, ["Stone"] = 0, ["Flint"] = 0, ["LeatherScrap"] = 0, ["DeerHide"] = 0,
            ["Feather"] = 0, ["Honey"] = 0, ["Raspberry"] = 0, ["Blueberries"] = 0, ["Mushroom"] = 0,
            ["Dandelion"] = 0, ["Resin"] = 0, ["FineWood"] = 0, ["Meat"] = 0, ["NeckTail"] = 0,
            ["BoarMeat"] = 0, ["DeerMeat"] = 0, ["QueenBee"] = 0,
            // Black Forest
            ["CopperOre"] = 1, ["TinOre"] = 1, ["Bronze"] = 1, ["CoreWood"] = 1, ["SurtlingCore"] = 1,
            ["TrollHide"] = 1, ["GreyDwarfEye"] = 1, ["ElderTrophy"] = 1,
            // Swamp
            ["IronScrap"] = 2, ["Iron"] = 2, ["AncientBark"] = 2, ["Guck"] = 2, ["Bloodbag"] = 2,
            ["Ooze"] = 2, ["Chain"] = 2, ["WitheredBone"] = 2,
            // Mountain
            ["SilverOre"] = 3, ["Silver"] = 3, ["WolfFang"] = 3, ["WolfPelt"] = 3, ["Obsidian"] = 3,
            ["FreezeGland"] = 3, ["Crystal"] = 3, ["WolfMeat"] = 3,
            // Plains
            ["BlackMetalScrap"] = 4, ["BlackMetal"] = 4, ["Flax"] = 4, ["Barley"] = 4, ["LoxPelt"] = 4,
            ["Needle"] = 4, ["Tar"] = 4, ["LoxMeat"] = 4,
            // Mistlands
            ["BlackMarble"] = 5, ["Sap"] = 5, ["SoftTissue"] = 5, ["Carapace"] = 5, ["YggdrasilWood"] = 5,
            ["Eitr"] = 5, ["ScaleHide"] = 5, ["Mandible"] = 5, ["HareMeat"] = 5,
            // Ashlands
            ["FlametalOre"] = 6, ["Flametal"] = 6, ["CharredBone"] = 6, ["CharredBlood"] = 6,
            ["Ashwood"] = 6, ["ProustitePowder"] = 6, ["AsksvinMeat"] = 6,
            // Deep North: no standard raw materials yet
        };

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

                if (IngredientRegion.TryGetValue(requirement.m_resItem.name, out int region) && region > best)
                    best = region;
            }

            return best;
        }

        /// <summary>
        /// Resolves a region's color: the selected palette's hex list acts as
        /// an index-aligned override - a non-empty valid hex replaces the
        /// built-in color, anything missing/empty/invalid keeps the built-in
        /// one. Parsed per call so config reloads apply live.
        /// </summary>
        private static Color ResolveRegionColor(int region)
        {
            var options = GlobalState.Config.RecipeOptions;
            bool bright = string.Equals(options.ColorPalette, "Bright", StringComparison.OrdinalIgnoreCase);
            string[] builtIn = bright ? BuiltInBrightColorsHex : BuiltInNatureColorsHex;

            string hex = null;
            var overrides = bright ? options.BrightColorsHex : options.NatureColorsHex;
            if (overrides != null && region < overrides.Count)
                hex = (overrides[region] ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(hex))
                hex = builtIn[region];

            if (!hex.StartsWith("#"))
                hex = "#" + hex;

            if (ColorUtility.TryParseHtmlString(hex, out Color color))
                return color;

            ColorUtility.TryParseHtmlString(builtIn[region], out color);
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
