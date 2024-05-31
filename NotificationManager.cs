using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
