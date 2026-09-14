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
        /// Force-unlock every recipe at session start. Opt-in extra hack:
        /// requires Debug mode AND this flag (was previously implied by Debug).
        /// </summary>
        public bool UnlockAllDebug { get; set; } = false;
    }
}
