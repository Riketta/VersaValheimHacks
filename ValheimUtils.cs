using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VersaValheimHacks
{
    internal class ValheimUtils
    {
        public static bool IsObjectPlayer(GameObject gameObject)
        {
            Player player = gameObject.GetComponent<Player>();

            if (player is null)
                return false;

            string unitName = player.GetPlayerName();
            string playerName = GlobalState.Player?.GetPlayerName();

            return !string.IsNullOrEmpty(playerName) && playerName == unitName;
        }

        public static Component[] GetAllComponents(GameObject gameObject)
        {
            Component[] components = gameObject.GetComponents(typeof(Component));
            
            return components;
        }
    }
}
