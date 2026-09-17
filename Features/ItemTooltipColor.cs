using System;
using UnityEngine;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Colors the name line of item tooltips with the item's region color -
    /// the same region map and palette the crafting panel sorting uses.
    /// Items with no mapped region keep the vanilla color. The name is
    /// stored as a raw localization token, so it is wrapped in a rich-text
    /// color tag that survives the game's own localization pass. Enabled
    /// via RecipeOptions.ColorItemTooltips.
    /// </summary>
    internal static class ItemTooltipColor
    {
        public static void Apply(ItemDrop.ItemData item, UITooltip tooltip)
        {
            try
            {
                if (item is null || tooltip is null || !GlobalState.Config.RecipeOptions.ColorItemTooltips)
                    return;

                string prefabName = item.m_dropPrefab != null ? item.m_dropPrefab.name : null;
                if (!CraftingSort.TryGetRegionColor(prefabName, out Color color))
                    return;

                tooltip.m_topic = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{tooltip.m_topic}</color>";
            }
            catch (Exception ex)
            {
                HarmonyLog.Log($"[ItemTooltipColor] Exception: {ex}.");
            }
        }
    }
}
