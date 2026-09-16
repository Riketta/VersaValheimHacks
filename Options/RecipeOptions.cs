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
        /// Force-unlock every recipe at session start. Opt-in extra hack:
        /// requires Debug mode AND this flag (was previously implied by Debug).
        /// </summary>
        public bool UnlockAllDebug { get; set; } = false;
    }
}
