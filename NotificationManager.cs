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

        public static bool Notification(string message, MessageHud.MessageType messageType = MessageHud.MessageType.Center)
        {
            if (GlobalState.Player is null)
                return false;

            GlobalState.Player.Message(messageType, message);

            return true;
        }

        public static bool SendToNearbyPlayers(string message, float radius, MessageHud.MessageType messageType = MessageHud.MessageType.Center)
        {
            if (GlobalState.Player is null)
                return false;

            List<Player> players = new List<Player>();
            GetPlayersInRange(GlobalState.Player.transform.position, radius, players);
            foreach (Player player in players)
                player.Message(messageType, message);

            return true;
        }
    }
}
