using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Area version of the vanilla chest stack (hover a chest and use it):
    /// walks every container within the configured radius and triggers the
    /// game's own Container.StackAll on each. That vanilla flow is fully
    /// multiplayer-safe - it requests container ownership, and the owner
    /// enforces the in-use and privacy checks before granting - so this mod
    /// only points it at multiple chests at once. The merge itself stays
    /// vanilla: only items that already have a stack in the target chest are
    /// moved, equipped items are skipped, and the game shows its own
    /// "stacked N items" message per chest.
    /// </summary>
    internal static class AreaStack
    {
        private static readonly FieldInfo AllPiecesField = AccessTools.Field(typeof(Piece), "s_allPieces");

        public static void StackToNearbyChests()
        {
            if (GlobalState.Player is null)
            {
                HarmonyLog.Log("[AreaStack] Can't stack: no player instance saved!");
                return;
            }

            var options = GlobalState.Config.AreaStackOptions;
            if (!options.Enabled)
                return;

            float radius = options.Radius;
            var containers = FindContainers(radius);
            if (containers.Count == 0)
            {
                NotificationManager.Notification($"No chests within {radius:0} m.", MessageHud.MessageType.TopLeft);
                HarmonyLog.Log($"[AreaStack] No containers within {radius:0} m.");
                return;
            }

            int triggered = 0;
            foreach (var container in containers)
            {
                try
                {
                    // Vanilla public entry point: requests ownership, the owner
                    // validates in-use/privacy, then merges on this client.
                    container.StackAll();
                    triggered++;
                    HarmonyLog.Log($"[AreaStack] Stack request sent: {container.gameObject.name}.");
                }
                catch (Exception ex)
                {
                    HarmonyLog.Log($"[AreaStack] Stack failed for {container.gameObject.name}: {ex}.");
                }
            }

            NotificationManager.Notification($"Stacking into {triggered} chest(s) within {radius:0} m.", MessageHud.MessageType.TopLeft);
            HarmonyLog.Log($"[AreaStack] Stack requests sent to {triggered}/{containers.Count} container(s) within {radius:0} m.");
        }

        private static List<Container> FindContainers(float radius)
        {
            var result = new List<Container>();
            var seen = new HashSet<Container>();
            var pieces = AllPiecesField?.GetValue(null) as List<Piece>;
            if (pieces is null)
            {
                HarmonyLog.Log("[AreaStack] Piece.s_allPieces reflection field is broken; cannot scan for containers.");
                return result;
            }

            Vector3 playerPosition = GlobalState.Player.transform.position;
            foreach (var piece in pieces)
            {
                if (piece is null)
                    continue;

                Container container = piece.GetComponent<Container>();
                if (container is null)
                    container = piece.GetComponentInParent<Container>();

                if (container is null || !seen.Add(container))
                    continue;

                if (Vector3.Distance(playerPosition, container.transform.position) <= radius)
                    result.Add(container);
            }

            result.Sort((a, b) => Vector3.Distance(playerPosition, a.transform.position)
                .CompareTo(Vector3.Distance(playerPosition, b.transform.position)));
            return result;
        }
    }
}
