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
            Player unit = gameObject.GetComponent<Player>();

            if (unit is null)
                return false;
            
            string playerName = GlobalState.Player?.GetPlayerName();

            if (string.IsNullOrEmpty(playerName))
                return false;

            string unitName = unit.GetPlayerName();

            return playerName == unitName;
        }

        public static Component[] GetAllComponents(GameObject gameObject)
        {
            Component[] components = gameObject.GetComponents(typeof(Component));
            
            return components;
        }
    }
}
