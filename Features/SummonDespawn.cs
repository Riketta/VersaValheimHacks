using System;
using System.Linq;
using UnityEngine;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Despawns every friendly skeleton the local player summoned (Numpad6).
    ///
    /// Safety: a character is only touched when ALL of the following hold -
    /// prefab name starts with "Skeleton_Friendly" (player summons; wild
    /// skeletons use other prefabs), its MonsterAI follow target resolves to
    /// a Player, and that player's ID equals the local player's. Wild
    /// creatures, other players' minions and everything else never match.
    /// Despawn goes through vanilla ZNetView.Destroy, so it stays
    /// multiplayer-safe (ownership RPC handled by the game).
    /// </summary>
    internal static class SummonDespawn
    {
        private const string FriendlySkeletonPrefix = "Skeleton_Friendly";

        public static void DespawnOwnedSkeletons()
        {
            try
            {
                Player player = GlobalState.Player ?? Player.m_localPlayer;
                if (player is null)
                {
                    NotificationManager.Notification("No player - can't despawn summons.", MessageHud.MessageType.TopLeft);
                    return;
                }

                long playerId = player.GetPlayerID();
                int despawned = 0;
                foreach (Character character in Character.GetAllCharacters().ToArray())
                {
                    if (character is null || character.IsDead())
                        continue;

                    if (!character.name.StartsWith(FriendlySkeletonPrefix, StringComparison.Ordinal))
                        continue;

                    var monsterAI = character.GetComponent<MonsterAI>();
                    GameObject followTarget = monsterAI != null ? monsterAI.GetFollowTarget() : null;
                    Player owner = followTarget != null ? followTarget.GetComponent<Player>() : null;
                    if (owner is null || owner.GetPlayerID() != playerId)
                        continue;

                    ZNetView netView = character.GetComponent<ZNetView>();
                    if (netView is null || !netView.IsValid())
                        continue;

                    netView.Destroy();
                    despawned++;
                }

                HarmonyLog.Log($"[SummonDespawn] Despawned {despawned} summoned skeleton(s).");
                NotificationManager.Notification(
                    despawned > 0
                        ? $"Despawned {despawned} summoned skeleton(s)."
                        : "No summoned skeletons found.",
                    MessageHud.MessageType.TopLeft);
            }
            catch (Exception ex)
            {
                HarmonyLog.Log($"[SummonDespawn] Exception: {ex}.");
            }
        }
    }
}
