namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Reveals crafting recipes when any single ingredient has been picked up
    /// once, instead of requiring every ingredient. Only affects recipe
    /// discovery - crafting costs and station requirements stay vanilla.
    /// </summary>
    internal static class RecipeDiscovery
    {
        public static void RevealBySingleIngredient(Player player, Recipe recipe, bool discover, ref bool result)
        {
            if (result || !discover || recipe is null || player is null)
                return;

            if (!GlobalState.ToggleHacks || !GlobalState.Config.RecipeOptions.RevealBySingleIngredient)
                return;

            foreach (Piece.Requirement requirement in recipe.m_resources)
            {
                if (requirement.m_resItem is null || requirement.m_amount <= 0)
                    continue;

                if (player.IsKnownMaterial(requirement.m_resItem.m_itemData.m_shared.m_name))
                {
                    result = true;
                    return;
                }
            }
        }
    }
}
