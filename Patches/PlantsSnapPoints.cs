using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static MeleeWeaponTrail;

namespace VersaValheimHacks.Patches
{
    internal class PlantsSnapPoints
    {
        public static string PointsToString(List<Transform> points)
        {
            return string.Join(",\n", points.Select(point => $"{point.name} = {point.position}"));
        }

        //[HarmonyPatch(typeof(UnityEngine.Random), "Range", new Type[] { typeof(int), typeof(int) })]
        internal class RandomRange
        {
            private const string Prefix = "Random.Range";

            [HarmonyPostfix]
            public static void DebugRandomRange(int __result, int minInclusive, int maxExclusive)
            {
                if (minInclusive == 0 && maxExclusive == 16)
                {
                    HarmonyLog.Log($"[{Prefix}.Postfix] Random.Range: {__result}.");
                    HarmonyLog.DumpStackTrace();
                }
            }
        }

        //[HarmonyPatch(typeof(Player), "SetupPlacementGhost")]
        internal class SetupPlacementGhost
        {
            private const string Prefix = "Player.SetupPlacementGhost";

            [HarmonyPostfix]
            public static void DebugSetupPlacementGhost(Player __instance, ref int ___m_placeRotation)
            {
                HarmonyLog.Log($"[{Prefix}.Postfix] Rotation step: {___m_placeRotation}.");
            }
        }

        //[HarmonyPatch(typeof(Player), "UpdatePlacement")]
        internal class UpdatePlacement
        {
            private const string Prefix = "Player.UpdatePlacement";

            [HarmonyPostfix]
            public static void DebugUpdatePlacement(Player __instance, ref int ___m_placeRotation)
            {
                HarmonyLog.Log($"[{Prefix}.Postfix] Rotation step: {___m_placeRotation}.");
            }
        }

        //[HarmonyPatch(typeof(Player), "UpdatePlacementGhost")]
        internal class UpdatePlacementGhost
        {
            private const string Prefix = "Player.UpdatePlacementGhost";

            [HarmonyPostfix]
            public static void DebugUpdatePlacementGhost(Player __instance, ref int ___m_placeRotation)
            {
                HarmonyLog.Log($"[{Prefix}.Postfix] Rotation step: {___m_placeRotation}.");
            }
        }

        //[HarmonyPatch(typeof(Player), "FindClosestSnapPoints")]
        internal class FindClosestSnapPoints
        {
            private const string Prefix = "Player.FindClosestSnapPoints";

            [HarmonyPostfix]
            public static void DebugFindClosestSnapPoints(Transform ghost, float maxSnapDistance, Transform a, Transform b, List<Piece> pieces)
            {
                HarmonyLog.Log($"[{Prefix}.Postfix] Ghost: {ghost.position}; Radius: {maxSnapDistance}; A: {a}; B: {b}; Pieces: [{string.Join(", ", pieces.Select(piece => piece.m_name))}].");
            }
        }

        //[HarmonyPatch(typeof(Player), "FindClosestSnappoint")]
        internal class FindClosestSnappoint
        {
            private const string Prefix = "Player.FindClosestSnapPoint";

            [HarmonyPostfix]
            public static void DebugFindClosestSnappoint(Vector3 p, List<Transform> snapPoints, float maxDistance, Transform closest, float distance)
            {
                HarmonyLog.Log($"[{Prefix}.Postfix] Snap point position: {p}; Max distance: {maxDistance}; Closest: {closest}; Distance: {distance}; Snap points:\n{PointsToString(snapPoints)}.");
            }
        }

        [HarmonyPatch(typeof(PieceTable), "GetSelectedPrefab")]
        internal class GetSelectedPrefab
        {
            private const string Prefix = "PieceTable.GetSelectedPrefab";

            [HarmonyPostfix]
            public static void DebugGetSelectedPrefab(PieceTable __instance)
            {
                //Piece piece = __instance.GetSelectedPiece();
                //if (piece != null)
                //{
                //    HarmonyLog.Log($"[{Prefix}.Postfix] PieceTable: {__instance.name}; Category: {__instance.m_selectedCategory}.");
                //    HarmonyLog.Log($"[{Prefix}.Postfix] Piece: {piece.m_name} ({piece.name}); Tag: {piece.tag}; Random rotation: {piece.m_randomInitBuildRotation}.");
                //}

                foreach (GameObject gameObject in __instance.m_pieces)
                {
                    Piece pieceComponent = gameObject.GetComponent<Piece>();
                    //HarmonyLog.Log($"[{Prefix}.Postfix] Piece from PieceTable: {pieceComponent.m_name} ({pieceComponent.name}); Tag: {pieceComponent.tag}; Random rotation: {pieceComponent.m_randomInitBuildRotation}.");

                    if (pieceComponent != null && GlobalState.Config.PiecesOptions.PlantPieces.ContainsKey(pieceComponent.m_name))
                    {
                        //Plant plant = gameObject.GetComponent<Plant>();
                        //if (plant != null)
                        //    HarmonyLog.Log($"[{Prefix}.Postfix] Plant: {plant.m_name} ({plant.name}); Tag: {plant.tag}; Grow radius: {plant.m_growRadius} or {plant.m_growRadiusVines}; Min scale: {plant.m_minScale}; Max scale: {plant.m_maxScale}.");

                        pieceComponent.m_randomInitBuildRotation = false;
                    }
                }
            }
        }

        //[HarmonyPatch(typeof(Piece), MethodType.Constructor)]
        internal class PieceConstructor
        {
            private const string Prefix = "Piece.Constructor";

            [HarmonyPostfix]
            public static void DebugPieceConstructor(Piece __instance)
            {
                HarmonyLog.Log($"[{Prefix}.Postfix] Piece: {__instance.m_name} ({__instance.name}); Random Rotation: {__instance.m_randomInitBuildRotation}.");
                __instance.m_randomInitBuildRotation = false;
                HarmonyLog.DumpStackTrace();
            }
        }

        //[HarmonyPatch(typeof(Piece), "GetSnapPoints", new Type[] { typeof(Vector3), typeof(float), typeof(List<Transform>), typeof(List<Piece>) })]
        internal class GetSnapPointsA
        {
            private const string Prefix = "Piece.GetNeighboursSnapPoints";

            /// <summary>
            /// Get snap points of current piece neighbours.
            /// </summary>
            [HarmonyPostfix]
            public static void DebugGetSnapPoints(Vector3 point, float radius, List<Transform> points, List<Piece> pieces)
            {
                HarmonyLog.Log($"[{Prefix}.Postfix] Point: {point}; Radius: {radius}; Pieces: [{string.Join(", ", pieces.Select(piece => piece.m_name))}]; Points:\n[{PointsToString(points)}].");
            }
        }

        [HarmonyPatch(typeof(Piece), "GetSnapPoints", new Type[] { typeof(List<Transform>) })]
        internal class GetSnapPointsB
        {
            private const string Prefix = "Piece.GetThisPieceSnapPoints";

            /// <summary>
            /// Get snap points of current piece.
            /// </summary>
            [HarmonyPostfix]
            public static void DebugGetSnapPoints(Piece __instance, List<Transform> points)
            {
                //HarmonyLog.Log($"[{Prefix}.Postfix] Piece ({__instance.m_name} ({__instance.name})): {__instance.transform.position}; Points:\n[{PointsToString(points)}].");

                Plant plant = __instance.gameObject.GetComponent<Plant>();
                if (plant is null)
                    return;

                int snappoints = 0;
                for (int i = 0; i < __instance.transform.childCount; i++)
                {
                    Transform child = __instance.transform.GetChild(i);

                    if (child.CompareTag("snappoint"))
                    {
                        //HarmonyLog.Log($"[{Prefix}.Postfix] Piece snap point: {child.name} = {child.position}.");
                        snappoints++;
                    }
                }

                if (snappoints == 0)
                {
                    float growRadius = 0f;
                    float colliderRadius = 0f;

                    growRadius = plant.m_growRadius;

                    Collider collider = plant.GetComponent<Collider>();
                    if (collider != null)
                        colliderRadius += collider.bounds.extents.x;

                    float radius = (growRadius + colliderRadius) * 1.15f; // 1.1f (max scale) enought, but rarely it fails.
                    HarmonyLog.Log($"[{Prefix}.Postfix] Plant: {__instance.m_name}; Radius: {radius}; Collider Radius: {growRadius}; Grow Radius: {colliderRadius}.");

                    GameObject gameObjectInner = new GameObject("Inner");
                    gameObjectInner.transform.SetParent(__instance.transform);
                    gameObjectInner.transform.rotation = __instance.transform.rotation;
                    gameObjectInner.transform.position = __instance.transform.position;
                    gameObjectInner.tag = "snappoint";
                    points.Add(gameObjectInner.transform);

                    GameObject gameObjectOuter = new GameObject("Outer");
                    gameObjectOuter.transform.SetParent(__instance.transform);
                    gameObjectOuter.transform.rotation = __instance.transform.rotation;
                    gameObjectOuter.transform.position = __instance.transform.position + (__instance.transform.right * radius);
                    gameObjectOuter.tag = "snappoint";
                    points.Add(gameObjectOuter.transform);
                }
            }
        }

        //[HarmonyPatch(typeof(Plant), "HaveGrowSpace")]
        internal class HaveGrowSpace
        {
            private const string Prefix = "Plant.HaveGrowSpace";

            [HarmonyPostfix]
            public static void DebugHaveGrowSpace(Plant __instance)
            {
                Collider collider = __instance.GetComponent<Collider>();
                if (collider != null)
                    HarmonyLog.Log($"[{Prefix}.Postfix] Collider radius: {collider.bounds.extents.x}; {collider.bounds.max.x}.");

                HarmonyLog.Log($"[{Prefix}.Postfix] Plant: {__instance.m_name} ({__instance.name}); Grow Radius: {__instance.m_growRadius}; Grow Radius Vines: {__instance.m_growRadiusVines}; Min scale: {__instance.m_minScale}; Max scale: {__instance.m_maxScale}; Collider: {collider?.bounds.ToString() ?? "-"}.");
            }
        }

        //[HarmonyPatch(typeof(Plant), MethodType.Constructor)]
        internal class PlantConstructor
        {
            private const string Prefix = "Plant.Constructor";

            [HarmonyPostfix]
            public static void DebugPlantConstructor(Plant __instance)
            {
                HarmonyLog.Log($"[{Prefix}.Postfix] Plant: {__instance.m_name} ({__instance.name}); Grow Radius: {__instance.m_growRadius}; Grow Radius Vines: {__instance.m_growRadiusVines}.");
            }
        }
    }
}
