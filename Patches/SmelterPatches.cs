using HarmonyLib;
using VersaValheimHacks.Features;

namespace VersaValheimHacks.Patches
{
    /// <summary>
    /// Scales processing station capacity (ore queue + fuel) when a station
    /// spawns.
    /// </summary>
    [HarmonyPatch(typeof(Smelter), "Awake")]
    internal class Smelter_Awake
    {
        private static void Postfix(Smelter __instance) => StationCapacity.Apply(__instance);
    }
}
