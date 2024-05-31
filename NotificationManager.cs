using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VersaValheimHacks
{
    internal class NotificationManager
    {
        public static bool Notification(string message, MessageHud.MessageType messageType = MessageHud.MessageType.Center)
        {
            if (GlobalState.Player is null)
                return false;

            GlobalState.Player.Message(messageType, message);

            return true;
        }

        public static bool SendNotificationToNerabyPlayers(string message, float radius, MessageHud.MessageType messageType = MessageHud.MessageType.Center)
        {
            if (GlobalState.Player is null)
                return false;

            MethodInfo GetPlayersInRangeMethod = AccessTools.Method(typeof(Player), "GetPlayersInRange");
            var GetPlayersInRange = AccessTools.MethodDelegate<Action<Vector3, float, List<Player>>>(GetPlayersInRangeMethod, GlobalState.Player);

            List<Player> list = new List<Player>();
            GetPlayersInRange(GlobalState.Player.transform.position, radius, list);
            foreach (Player player in list)
                player.Message(messageType, message);

            return true;
        }
    }
}
