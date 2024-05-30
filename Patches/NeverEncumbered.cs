using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaValheimHacks.Patches
{
    internal class NeverEncumbered
    {
        [HarmonyPatch(typeof(Player), "IsEncumbered")]
        internal class IsEncumbered
        {
            private const string Prefix = "Player.IsEncumbered";

            [HarmonyPostfix]
            public static void NeverEncumbered(ref bool __result)
            {
                if (!GlobalState.ToggleHacks || !GlobalState.Config.GodModeOptions.NeverEncumbered)
                    return;

                //HarmonyLog.Log($"[{Prefix}.Postfix] IsEncumbered (real): {__result}.");
                __result = false;
            }
        }

        [HarmonyPatch(typeof(Player), "GetMaxCarryWeight")]
        internal class GetMaxCarryWeight
        {
            private const string Prefix = "Player.GetMaxCarryWeight";

            [HarmonyPostfix]
            public static void MoreMaxCarryWeight(ref float __result)
            {
                if (!GlobalState.ToggleHacks || !GlobalState.Config.GodModeOptions.NeverEncumbered)
                    return;

                //HarmonyLog.Log($"[{Prefix}.Postfix] MaxCarryWeight (real): {__result}.");
                __result *= GlobalState.Config.GodModeOptions.CarryWeightMultipiler;
            }
        }
    }
}
