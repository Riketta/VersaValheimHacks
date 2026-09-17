using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Auto-planting: plant two same-type crops, mark them as opposite
    /// corners of a field (Numpad7 = corner A, Numpad8 = corner B + start),
    /// and the planter fills the rectangle with that crop using the same
    /// per-crop spacing as the snap-point chain planting. One seed per plant
    /// (matched from the sapling piece's requirements) is consumed from the
    /// inventory; planting stops when the seeds run out. Already-occupied
    /// spots are skipped, so re-running only fills gaps. Corner B is treated
    /// as the true opposite corner - its offset is snapped to whole spacing
    /// steps, so sloppy marking just rounds the field size.
    /// </summary>
    internal static class AutoPlant
    {
        private const float MarkRange = 4f;
        private const float OccupiedRadius = 0.3f;

        private static readonly FieldInfo AllPiecesField =
            AccessTools.Field(typeof(Piece), "s_allPieces");

        private static Vector3 _cornerA;
        private static string _cropPrefab;
        private static bool _hasCornerA;
        private static Vector2 _forward;
        private static Vector2 _right;

        public static void MarkFirst()
        {
            try
            {
                Player player = GlobalState.Player ?? Player.m_localPlayer;
                if (player is null)
                    return;

                Piece piece = FindNearestPlant(player, out float distance);
                if (piece is null)
                {
                    NotificationManager.Notification($"No planted crop within {MarkRange:0} m to mark.", MessageHud.MessageType.TopLeft);
                    return;
                }

                _cornerA = piece.transform.position;
                _cropPrefab = CleanPrefabName(piece.gameObject.name);
                _hasCornerA = true;

                // Lock the rectangle orientation to the player's view: corner
                // A is 'top-left' and corner B 'bottom-right' as seen on screen.
                var camera = GameCamera.instance;
                Vector3 look = camera != null ? camera.transform.forward : player.transform.forward;
                look.y = 0f;
                if (look.sqrMagnitude < 0.001f)
                    look = player.transform.forward;
                look.y = 0f;
                look.Normalize();
                _forward = new Vector2(look.x, look.z);
                _right = new Vector2(_forward.y, -_forward.x);

                NotificationManager.Notification($"Auto-plant corner A: {CropDisplayName(piece)} ({distance:0.0} m).", MessageHud.MessageType.TopLeft);
            }
            catch (Exception ex)
            {
                HarmonyLog.Log($"[AutoPlant] MarkFirst exception: {ex}.");
            }
        }

        public static void MarkSecondAndPlant()
        {
            try
            {
                Player player = GlobalState.Player ?? Player.m_localPlayer;
                if (player is null || !_hasCornerA)
                {
                    NotificationManager.Notification("Mark corner A first (Numpad7).", MessageHud.MessageType.TopLeft);
                    return;
                }

                Piece piece = FindNearestPlant(player, out float _);
                if (piece is null)
                {
                    NotificationManager.Notification($"No planted crop within {MarkRange:0} m to mark.", MessageHud.MessageType.TopLeft);
                    return;
                }

                string prefabB = CleanPrefabName(piece.gameObject.name);
                if (!string.Equals(prefabB, _cropPrefab, StringComparison.Ordinal))
                {
                    NotificationManager.Notification("Corner B must be the same crop type as corner A.", MessageHud.MessageType.TopLeft);
                    return;
                }

                Vector3 cornerB = piece.transform.position;

                // A and B are opposite corners (top-left -> bottom-right on
                // screen when marking). The rectangle is oriented by the view
                // captured at corner A; extents are snapped to whole spacing
                // steps, so sloppy corner placement just rounds the size.
                Vector2 delta = new Vector2(cornerB.x, cornerB.z) - new Vector2(_cornerA.x, _cornerA.z);
                float spacing = GetSpacing(piece);
                int stepRight = Math.Max(1, (int)Math.Round(Vector2.Dot(delta, _right) / spacing));
                int stepForward = Math.Max(1, (int)Math.Round(Vector2.Dot(delta, _forward) / spacing));
                Vector2 dirRight = _right * Math.Sign(stepRight);
                Vector2 dirForward = _forward * Math.Sign(stepForward);
                stepRight = Math.Abs(stepRight);
                stepForward = Math.Abs(stepForward);

                var positions = new List<Vector2>();
                for (int i = 0; i <= stepRight; i++)
                {
                    for (int j = 0; j <= stepForward; j++)
                    {
                        positions.Add(new Vector2(_cornerA.x, _cornerA.z)
                            + i * spacing * dirRight
                            + j * spacing * dirForward);
                    }
                }

                Vector2 playerPos = new Vector2(player.transform.position.x, player.transform.position.z);
                positions.Sort((x, y) => Vector2.SqrMagnitude(x - playerPos).CompareTo(Vector2.SqrMagnitude(y - playerPos)));

                string seedName = null;
                int planted = 0, occupied = 0;
                bool outOfSeeds = false;
                foreach (Vector2 position in positions)
                {
                    if (IsOccupied(position))
                    {
                        occupied++;
                        continue;
                    }

                    if (!ConsumeSeed(player, prefabB, ref seedName))
                    {
                        outOfSeeds = true;
                        break;
                    }

                    if (SpawnSapling(prefabB, position))
                        planted++;
                }

                string summary = $"Auto-planted {planted} (skipped {occupied} occupied) [{stepForward + 1}x{stepRight + 1}]";
                summary += outOfSeeds ? " - out of seeds!" : ".";
                HarmonyLog.Log($"[AutoPlant] {summary} Field: {stepForward + 1}x{stepRight + 1}, spacing {spacing:0.00} m.");
                NotificationManager.Notification(summary, MessageHud.MessageType.TopLeft);
            }
            catch (Exception ex)
            {
                HarmonyLog.Log($"[AutoPlant] MarkSecondAndPlant exception: {ex}.");
            }
        }

        private static Piece FindNearestPlant(Player player, out float bestDistance)
        {
            bestDistance = float.MaxValue;
            Piece best = null;
            if (!(AllPiecesField?.GetValue(null) is List<Piece> allPieces))
                return null;

            Vector3 origin = player.transform.position;
            foreach (Piece piece in allPieces)
            {
                if (piece is null || piece.GetComponent<Plant>() is null)
                    continue;

                float distance = Vector3.Distance(origin, piece.transform.position);
                if (distance < MarkRange && distance < bestDistance)
                {
                    best = piece;
                    bestDistance = distance;
                }
            }

            return best;
        }

        /// <summary>
        /// Center-to-center spacing matching the snap-point chain planting:
        /// the distance between a plant's center and its outer snap point.
        /// </summary>
        private static float GetSpacing(Piece cropPiece)
        {
            Plant plant = cropPiece.GetComponent<Plant>();
            if (plant is null)
                return 1f;

            float growRadius = Mathf.Max(plant.m_growRadius, plant.m_growRadiusVines);
            float colliderRadius = 0f;
            Collider collider = cropPiece.GetComponent<Collider>();
            if (collider != null)
                colliderRadius += collider.bounds.extents.x;

            return (growRadius + colliderRadius) * 1.1f
                * GlobalState.Config.PiecesOptions.PlantExtraRadiusMultiplier;
        }

        private static bool IsOccupied(Vector2 position)
        {
            if (!(AllPiecesField?.GetValue(null) is List<Piece> allPieces))
                return false;

            foreach (Piece piece in allPieces)
            {
                if (piece is null)
                    continue;

                Vector2 piecePos = new Vector2(piece.transform.position.x, piece.transform.position.z);
                if ((piecePos - position).sqrMagnitude < OccupiedRadius * OccupiedRadius)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Consumes one seed for the crop: exact match from the sapling
        /// piece's requirements, falling back to the first seeds found in
        /// the inventory.
        /// </summary>
        private static bool ConsumeSeed(Player player, string cropPrefab, ref string seedName)
        {
            Inventory inventory = player.GetInventory();
            if (inventory is null)
                return false;

            if (seedName is null)
            {
                GameObject prefab = ZNetScene.instance != null ? ZNetScene.instance.GetPrefab(cropPrefab) : null;
                Piece piece = prefab != null ? prefab.GetComponent<Piece>() : null;
                if (piece?.m_resources != null)
                {
                    foreach (Piece.Requirement requirement in piece.m_resources)
                    {
                        if (requirement?.m_resItem != null && requirement.m_amount > 0)
                        {
                            seedName = requirement.m_resItem.name;
                            break;
                        }
                    }
                }
            }

            if (seedName != null && HasItem(inventory, seedName))
            {
                inventory.RemoveItem(seedName, 1);
                return true;
            }

            // Fallback: first seeds found in the bag.
            foreach (ItemDrop.ItemData item in inventory.GetAllItems())
            {
                string itemPrefab = item.m_dropPrefab != null ? item.m_dropPrefab.name : null;
                if (itemPrefab != null && itemPrefab.IndexOf("Seeds", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    seedName = itemPrefab;
                    inventory.RemoveItem(seedName, 1);
                    return true;
                }
            }

            return false;
        }

        private static bool HasItem(Inventory inventory, string prefabName)
        {
            foreach (ItemDrop.ItemData item in inventory.GetAllItems())
            {
                if (item.m_dropPrefab != null && item.m_dropPrefab.name == prefabName && item.m_stack > 0)
                    return true;
            }

            return false;
        }

        private static bool SpawnSapling(string cropPrefab, Vector2 position)
        {
            ZNetScene zoneScene = ZNetScene.instance;
            GameObject prefab = zoneScene != null ? zoneScene.GetPrefab(cropPrefab) : null;
            if (prefab is null)
                return false;

            ZoneSystem zoneSystem = ZoneSystem.instance;
            float groundY = zoneSystem != null
                ? zoneSystem.GetGroundHeight(new Vector3(position.x, 0f, position.y))
                : position.y;

            UnityEngine.Object.Instantiate(prefab, new Vector3(position.x, groundY, position.y), Quaternion.identity);
            return true;
        }

        private static string CleanPrefabName(string gameObjectName)
        {
            return (gameObjectName ?? string.Empty).Replace("(Clone)", string.Empty).Trim();
        }

        private static string CropDisplayName(Piece piece)
        {
            Localization localization = Localization.instance;
            return localization != null ? localization.Localize(piece.m_name) : piece.m_name;
        }
    }
}
