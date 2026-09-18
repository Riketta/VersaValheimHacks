using HarmonyLib;

using UnityEngine.UI;

using VersaValheimHacks.Features;



namespace VersaValheimHacks.Patches

{

    /// <summary>
    /// Adds the container "Sort" button once the inventory GUI is built.
    /// </summary>
    [HarmonyPatch(typeof(InventoryGui), "Awake")]
    internal class InventoryGui_Awake
    {
        private static void Postfix(InventoryGui __instance)
        {
            ContainerSort.EnsureButton(__instance);
        }
    }

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
    /// region lookup never goes stale.
    /// </summary>
    [HarmonyPatch(typeof(InventoryGrid), "CreateItemTooltip")]
    internal class InventoryGrid_CreateItemTooltip
    {
        private static void Postfix(ItemDrop.ItemData item, UITooltip tooltip)
        {
            ItemTooltipColor.Apply(item, tooltip);
        }
    }

    /// <summary>
    /// Tints the visible tooltip's Topic label while it is on screen.
    /// </summary>
    [HarmonyPatch(typeof(UITooltip), "UpdateTextElements")]
    internal class UITooltip_UpdateTextElements
    {
        private static void Postfix(UITooltip __instance)
        {
            ItemTooltipColor.ApplyToTextElements(__instance);
        }
    }
}
