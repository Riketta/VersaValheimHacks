using UnityEngine.UI;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Crafting / upgrading / repairing without resources or station requirements.
    /// </summary>
    internal static class FreeCrafting
    {
        private static bool FeatureEnabled => GlobalState.ToggleHacks && GlobalState.Config.GodModeOptions.FreeCraftingEnabled;

        private static bool _upgradeTabLogged;

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

            if (!_upgradeTabLogged)
            {
                _upgradeTabLogged = true;
                HarmonyLog.Log("[FreeCrafting] Upgrade tab force-enabled.");
            }

            upgradeTab.gameObject.SetActive(true);
        }
    }
}
