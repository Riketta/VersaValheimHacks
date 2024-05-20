using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace VersaValheimHacks.Patches
{
    internal class UpdateCraftingPanel
    {
        [HarmonyPatch(typeof(InventoryGui), "UpdateCraftingPanel")]
        internal class GetGlobalKey
        {
            private const string Prefix = "InventoryGui.UpdateCraftingPanel";
            public static bool Status = true;

            [HarmonyPostfix]
            public static void EnableCraftAndUpgradeButton(ref Button ___m_tabUpgrade)
            {
                if (!GlobalState.EnableHacks)
                    return;

                HarmonyLog.Log($"[{Prefix}.Postfix] Trying to enable upgrade button.");
                ___m_tabUpgrade.gameObject.SetActive(value: true);
            }
        }
    }
}
