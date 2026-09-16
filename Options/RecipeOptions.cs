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
        /// Color each crafting row label with its region's color from the
        /// fixed Nature palette; rows with no inferable region stay gray.
        /// Non-craftable rows keep the color dimmed.
        /// </summary>
        public bool ColorByRegion { get; set; } = true;

        /// <summary>
        /// Which region color palette to use: "Nature" (muted earth tones,
        /// the default) or "Bright" (rarity-style). Unknown names fall back
        /// to Nature.
        /// </summary>
        public string ColorPalette { get; set; } = "Nature";

        /// <summary>
        /// Hex overrides ("#RRGGBB") for the Bright palette. Index 0 =
        /// Meadows ... index 7 = Deep North. Empty/invalid entries keep the
        /// built-in color; the list may be shortened freely.
        /// </summary>
        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public List<string> BrightColorsHex { get; set; } = new List<string>
        {
            "#8C8C8C", "#FFFFFF", "#59FF59", "#59A6FF", "#BF66FF", "#FF9E33", "#FF4D4D", "#33FFFF",
        };

        /// <summary>
        /// Hex overrides ("#RRGGBB") for the Nature palette. Index 0 =
        /// Meadows ... index 7 = Deep North. Empty/invalid entries keep the
        /// built-in color; the list may be shortened freely.
        /// </summary>
        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public List<string> NatureColorsHex { get; set; } = new List<string>
        {
            "#8BC34A", "#2D5016", "#5B5A35", "#CFE8F0", "#D4AF37", "#7C6A8A", "#B23A2E", "#3F7EA6",
        };

        /// <summary>
        /// Force-unlock every recipe at session start. Opt-in extra hack:
        /// requires Debug mode AND this flag (was previously implied by Debug).
        /// </summary>
        public bool UnlockAllDebug { get; set; } = false;
    }
}
