using HarmonyLib;
using UnityEngine.UI;
using VersaValheimHacks.Features;

namespace VersaValheimHacks.Patches
{
    [HarmonyPatch(typeof(InventoryGui), "UpdateCraftingPanel")]
    internal class InventoryGui_UpdateCraftingPanel
    {
        private static void Postfix(InventoryGui __instance, Button ___m_tabUpgrade)
        {
            FreeCrafting.ForceUpgradeTabVisible(___m_tabUpgrade);
            CraftingSort.SortCraftingPanel(__instance);
            CraftingSort.ColorizeByRegion(__instance);
        }
    }

    /// <summary>
    /// Draws the stack-protection outline on marked item slots every time
    /// an inventory grid refreshes (runs each frame while a panel is open).
    /// </summary>
    [HarmonyPatch(typeof(InventoryGrid), "UpdateGui")]
    internal class InventoryGrid_UpdateGui
    {
        private static void Postfix(InventoryGrid __instance)
        {
            StackProtection.UpdateMarks(__instance);
        }
    }

    /// <summary>
    /// Applies the region color to the name line of item tooltips. Runs on
    /// every tooltip (re)build - vanilla resets the topic each call, so the
    /// color tag never accumulates.
    /// </summary>
    [HarmonyPatch(typeof(InventoryGrid), "CreateItemTooltip")]
    internal class InventoryGrid_CreateItemTooltip
    {
        private static void Postfix(ItemDrop.ItemData item, UITooltip tooltip)
        {
            ItemTooltipColor.Apply(item, tooltip);
        }
    }
}
