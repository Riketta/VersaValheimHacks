using HarmonyLib;
using UnityEngine.UI;
using VersaValheimHacks.Features;

namespace VersaValheimHacks.Patches
{
    [HarmonyPatch(typeof(InventoryGui), "UpdateCraftingPanel")]
    internal class InventoryGui_UpdateCraftingPanel
    {
        private static void Postfix(Button ___m_tabUpgrade) => FreeCrafting.ForceUpgradeTabVisible(___m_tabUpgrade);
    }
}
