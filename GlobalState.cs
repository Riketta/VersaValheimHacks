using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaValheimHacks
{
    internal class GlobalState
    {
        public static bool EnableHacks => WinApi.IsCapsLockOn;
        
        public static bool EnableExtraHacks => EnableHacks && IsPlayerCrouching;
        
        public static bool IsPlayerCrouching { get; set; }
    }
}
