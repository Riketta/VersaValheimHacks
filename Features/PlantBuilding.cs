using System.Collections.Generic;
using UnityEngine;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Building helpers for plants: snap points for proper spacing, no random
    /// initial rotation, and a HUD notification with the placement angle.
    /// </summary>
    internal static class PlantBuilding
    {
        private static int _lastPlaceRotation = int.MinValue;

        public static void AddPlantSnapPoints(Piece piece, List<Transform> points)
        {
            Plant plant = piece.gameObject.GetComponent<Plant>();
            if (plant is null)
                return;

            if (CountSnapPointChildren(piece.transform) > 0)
                return;

            float growRadius = Mathf.Max(plant.m_growRadius, plant.m_growRadiusVines);
            float colliderRadius = 0f;

            Collider collider = plant.GetComponent<Collider>();
            if (collider != null)
                colliderRadius += collider.bounds.extents.x;

            // 1.1 (max scale) is usually enough; add a small configurable margin on top.
            float radius = (growRadius + colliderRadius) * 1.1f * GlobalState.Config.PiecesOptions.PlantExtraRadiusMultiplier;

            AddSnapPoint(piece.transform, "Inner", piece.transform.position);
            AddSnapPoint(piece.transform, "Outer", piece.transform.position + piece.transform.right * radius);
        }

        public static void DisableRandomPlantRotation(PieceTable pieceTable)
        {
            foreach (GameObject gameObject in pieceTable.m_pieces)
            {
                Piece piece = gameObject.GetComponent<Piece>();
                if (piece != null && GlobalState.Config.PiecesOptions.PlantPieces.ContainsKey(piece.m_name))
                    piece.m_randomInitBuildRotation = false;
            }
        }

        public static void NotifyPlacementAngle(int placeRotation)
        {
            if (_lastPlaceRotation == placeRotation)
                return;

            float angle = Mathf.Abs(placeRotation % 16 * 22.5f); // Player.m_placeRotationDegrees
            NotificationManager.Notification($"Angle: {angle:F1}.", MessageHud.MessageType.TopLeft);
            _lastPlaceRotation = placeRotation;
        }

        private static int CountSnapPointChildren(Transform transform)
        {
            int count = 0;
            for (int i = 0; i < transform.childCount; i++)
                if (transform.GetChild(i).CompareTag("snappoint"))
                    count++;
            return count;
        }

        private static void AddSnapPoint(Transform parent, string name, Vector3 position)
        {
            var point = new GameObject(name);
            point.transform.SetParent(parent);
            point.transform.rotation = parent.rotation;
            point.transform.position = position;
            point.tag = "snappoint";
        }
    }
}
