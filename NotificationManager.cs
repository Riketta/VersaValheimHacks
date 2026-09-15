using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VersaValheimHacks
{
    internal static class NotificationManager
    {
        // Game method is static; the open delegate can be cached once.
        private static readonly Action<Vector3, float, List<Player>> GetPlayersInRange =
            AccessTools.MethodDelegate<Action<Vector3, float, List<Player>>>(AccessTools.Method(typeof(Player), nameof(Player.GetPlayersInRange)));

        public static bool Notification(string message, MessageHud.MessageType messageType = MessageHud.MessageType.Center, bool force = false)
        {
            // Streamer mode hides every mod-emitted message from viewers.
            if (!force && GlobalState.Config is { StreamerMode: true })
                return false;

            if (!TryGetLocalPlayer(out var player))
                return false;

            player.Message(messageType, message);

            return true;
        }

        public static bool SendToNearbyPlayers(string message, float radius, MessageHud.MessageType messageType = MessageHud.MessageType.Center)
        {
            if (!TryGetLocalPlayer(out var player))
                return false;

            List<Player> players = new List<Player>();
            GetPlayersInRange(player.transform.position, radius, players);
            foreach (Player nearby in players)
                nearby.Message(messageType, message);

            return true;
        }

        /// <summary>
        /// Unity-aware resolution: a destroyed cached player (scene unloaded)
        /// must fall back to the game's own local player reference.
        /// </summary>
        private static bool TryGetLocalPlayer(out Player player)
        {
            player = GlobalState.Player;
            if (player == null) // Unity-aware: also catches destroyed objects
                player = Player.m_localPlayer;

            return player != null;
        }
    }
}
