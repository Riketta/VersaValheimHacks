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
        /// Force-unlock every recipe at session start. Opt-in extra hack:
        /// requires Debug mode AND this flag (was previously implied by Debug).
        /// </summary>
        public bool UnlockAllDebug { get; set; } = false;
    }
}
