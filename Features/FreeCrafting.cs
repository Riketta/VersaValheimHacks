using UnityEngine.UI;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Crafting / upgrading / repairing without resources or station requirements.
    /// </summary>
    internal static class FreeCrafting
    {
        private static bool FeatureEnabled => GlobalState.ToggleHacks && GlobalState.Config.GodModeOptions.FreeCraftingEnabled;

        public static void ForceNoCost(ref bool noCost)
        {
            if (FeatureEnabled)
                noCost = true;
        }

        public static void ForceGlobalKey(GlobalKeys key, ref bool result)
        {
            if (FeatureEnabled && key == GlobalKeys.NoCraftCost)
                result = true;
        }

        public static void ForceUpgradeTabVisible(Button upgradeTab)
        {
            if (!FeatureEnabled)
                return;

            HarmonyLog.Log("[FreeCrafting] Enabling upgrade tab.");
            upgradeTab.gameObject.SetActive(true);
        }
    }
}
