using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Colors the name line of item tooltips with the item's region color -
    /// the same region map and palette the crafting panel sorting uses.
    /// Items with no mapped region keep the vanilla color.
    ///
    /// The color is applied directly to the tooltip's Topic TMP text (not
    /// via rich-text tags - tooltip texts may have rich text disabled) as a
    /// postfix on UITooltip.UpdateTextElements, which runs every frame the
    /// tooltip is visible. CreateItemTooltip feeds us the region per slot
    /// while the panel is open. Enabled via RecipeOptions.ColorItemTooltips.
    /// </summary>
    internal static class ItemTooltipColor
    {
        private static readonly FieldInfo TooltipInstanceField =
            AccessTools.Field(typeof(UITooltip), "m_tooltip");

        private static readonly Dictionary<UITooltip, Color> _colors = new Dictionary<UITooltip, Color>();
        private static readonly Dictionary<TMP_Text, Color> _originalColors = new Dictionary<TMP_Text, Color>();
        private static TMP_Text _topicText;

        public static void Apply(ItemDrop.ItemData item, UITooltip tooltip)
        {
            try
            {
                if (tooltip is null)
                    return;

                Color? color = null;
                if (item != null && GlobalState.Config.RecipeOptions.ColorItemTooltips)
                {
                    string prefabName = item.m_dropPrefab != null ? item.m_dropPrefab.name : null;
                    if (CraftingSort.TryGetRegionColor(prefabName, out Color resolved))
                    {
                        color = resolved;
                    }
                    else
                    {
                        // Crafted item: inherit the recipe's highest-region
                        // ingredient - the same idea as the crafting panel.
                        Recipe recipe = ObjectDB.instance != null ? ObjectDB.instance.GetRecipe(item) : null;
                        int region = CraftingSort.GetRecipeRegion(recipe);
                        if (region >= 0)
                            color = CraftingSort.GetRegionColor(region);
                    }

                    if (GlobalState.Config.Debug)
                        HarmonyLog.Log($"[ItemTooltipColor] {prefabName}: {(color.HasValue ? color.Value.ToString() : "no region")}.");
                }

                if (color.HasValue)
                    _colors[tooltip] = color.Value;
                else
                    _colors.Remove(tooltip);

                // Set() early-returns when topic/text are unchanged (which is
                // the common case while hovering a static item), so the
                // UpdateTextElements postfix alone would fire unreliably.
                // Tint right away - this runs every frame while hovering.
                ApplyToTextElements(tooltip);
            }
            catch (Exception ex)
            {
                HarmonyLog.Log($"[ItemTooltipColor] Apply exception: {ex}.");
            }
        }

        /// <summary>
        /// Postfix of UITooltip.UpdateTextElements: tint the Topic label of
        /// the currently shown tooltip, or restore its original color when
        /// the hovered item has no region.
        /// </summary>
        public static void ApplyToTextElements(UITooltip tooltip)
        {
            try
            {
                TMP_Text topicText = FindTopicText();
                if (topicText is null)
                    return;

                if (!_originalColors.TryGetValue(topicText, out Color original))
                {
                    _originalColors[topicText] = topicText.color;
                    original = topicText.color;
                }

                topicText.color = _colors.TryGetValue(tooltip, out Color color)
                    ? color
                    : original;
            }
            catch (Exception ex)
            {
                HarmonyLog.Log($"[ItemTooltipColor] TextElements exception: {ex}.");
            }
        }

        private static TMP_Text FindTopicText()
        {
            if (_topicText != null)
                return _topicText;

            if (!(TooltipInstanceField?.GetValue(null) is GameObject instance) || instance == null)
                return null;

            Transform topic = Utils.FindChild(instance.transform, "Topic");
            _topicText = topic != null ? topic.GetComponent<TMP_Text>() : null;
            return _topicText;
        }
    }
}
