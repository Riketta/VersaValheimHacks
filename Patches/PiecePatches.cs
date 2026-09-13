using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using VersaValheimHacks.Features;

namespace VersaValheimHacks.Patches
{
    [HarmonyPatch(typeof(Piece), nameof(Piece.GetSnapPoints), new[] { typeof(List<Transform>) })]
    internal class Piece_GetSnapPoints
    {
        private static void Postfix(Piece __instance, List<Transform> points) => PlantBuilding.AddPlantSnapPoints(__instance, points);
    }
}
