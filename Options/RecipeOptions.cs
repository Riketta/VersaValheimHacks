using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VersaValheimHacks.Options
{
    internal class RecipeOptions
    {
        /// <summary>
        /// Reveal a recipe when any single ingredient has been picked up once
        /// (vanilla requires every ingredient unless the recipe itself allows
        /// one). Master-toggle gated.
        /// </summary>
        public bool RevealBySingleIngredient { get; set; } = true;

        /// <summary>
        /// Sort the crafting panel by crafting tier (required station level,
        /// lowest first), then by progression region, then alphabetically.
        /// Overrides the vanilla sort modes.
        /// </summary>
        public bool SortCraftingPanel { get; set; } = true;

        /// <summary>
        /// Crafting list sort order: "Region, Tier, Alphabet" (default)
        /// groups by progression region first, then required station level,
        /// then name; "Tier, Region, Alphabet" keeps the station-level
        /// grouping first. Unknown values fall back to the default.
        /// </summary>
        public string SortOrder { get; set; } = "Region, Tier, Alphabet";

        /// <summary>
        /// Color each crafting row label with its region's color from the
        /// selected palette; rows with no inferable region stay gray.
        /// Non-craftable rows keep the color dimmed.
        /// </summary>
        public bool ColorByRegion { get; set; } = true;

        /// <summary>
        /// Which region color palette to use. Must match a key in
        /// ColorPalettes (case-insensitive); unknown names fall back to
        /// "Region". Add your own keys to ColorPalettes to create custom
        /// palettes, then select them here.
        /// </summary>
        public string ColorPalette { get; set; } = "RegionBright";

        /// <summary>
        /// Named region color palettes as hex values ("#RRGGBB", # optional;
        /// index 0 = Meadows ... index 7 = Deep North; a shorter list clamps
        /// to its last color). Keys are palette names - add your own and
        /// select them via ColorPalette.
        /// </summary>
        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public Dictionary<string, List<string>> ColorPalettes { get; set; } =
            new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["RegionBright"] = new List<string>
            {
                "#6DE04A", "#1A6125", "#B366FF", "#7FD4FF", "#FFC93C", "#5C7CFF", "#FF5A2E", "#45C8E8",
            },
            ["Region"] = new List<string>
            {
                "#8BC34A", "#2D5016", "#6B4E71", "#CFE8F0", "#D4AF37", "#3B4A6B", "#B23A2E", "#3F7EA6",
            },
            ["Rarity"] = new List<string>
            {
                "#8C8C8C", "#FFFFFF", "#59FF59", "#59A6FF", "#BF66FF", "#FF9E33", "#FF4D4D", "#33FFFF",
            },
        };

        /// <summary>
        /// Force-unlock every recipe at session start. Opt-in extra hack:
        /// requires Debug mode AND this flag (was previously implied by Debug).
        /// </summary>
        public bool UnlockAllDebug { get; set; } = false;
    }
}
