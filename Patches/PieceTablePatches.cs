using HarmonyLib;
using VersaValheimHacks.Features;

namespace VersaValheimHacks.Patches
{
    [HarmonyPatch(typeof(PieceTable), nameof(PieceTable.GetSelectedPrefab))]
    internal class PieceTable_GetSelectedPrefab
    {
        private static void Postfix(PieceTable __instance) => PlantBuilding.DisableRandomPlantRotation(__instance);
    }
}
