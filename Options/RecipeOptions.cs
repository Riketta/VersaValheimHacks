using System.Collections.Generic;

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
        /// Sort the crafting panel alphabetically by localized name, then by
        /// required station level (same-name upgrade recipes list lowest level
        /// first). Overrides the vanilla sort modes, which always group by
        /// craftable state and hand-placed category weights.
        /// </summary>
        public bool SortCraftingPanel { get; set; } = true;

        /// <summary>
        /// Color each crafting row label by its required station level:
        /// 1 gray, 2 white, 3 green, 4 blue, 5 purple, 6 orange, 7 red,
        /// 8+ cyan. Non-craftable rows keep the tier color dimmed.
        /// </summary>
        public bool ColorizeByTier { get; set; } = true;

        /// <summary>
        /// Which tier color layout to use: "Default" (bright rarity-style;
        /// also uses the custom TierColorsHex list) or "Nature" (muted earth
        /// tones). Unknown names fall back to Default.
        /// </summary>
        public string TierColorsLayout { get; set; } = "Default";

        /// <summary>
        /// Row label colors per station level as hex values ("#RRGGBB", #
        /// optional): index 0 = tier 1 ... index 7 = tier 8+. Tiers beyond
        /// the list share the last color; invalid entries fall back to the
        /// built-in palette.
        /// </summary>
        public List<string> TierColorsHex { get; set; } = new List<string>
        {
            "#8C8C8C", "#FFFFFF", "#59FF59", "#59A6FF", "#BF66FF", "#FF9E33", "#FF4D4D", "#33FFFF",
        };

        /// <summary>
        /// Force-unlock every recipe at session start. Opt-in extra hack:
        /// requires Debug mode AND this flag (was previously implied by Debug).
        /// </summary>
        public bool UnlockAllDebug { get; set; } = false;
    }
}
