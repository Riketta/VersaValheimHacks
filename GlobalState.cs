using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaValheimHacks
{
    internal class GlobalState
    {
        public static Config Config { get; set; }

        public static IntPtr GameWindowHandle { get; } = WindowsManager.GetCurrentThreadWindowHandle();

        public static bool ToggleHacks => Config != null && Config.Enabled;
        
        public static bool ToggleExtraHacks => ToggleHacks && IsPlayerCrouching;
        
        public static bool IsPlayerCrouching { get; set; }

        public static bool EnableDebugTools { get; set; } = false;
        public static World World { get; set; }
        public static ZoneSystem ZoneSystem { get; set; }
        public static PlayerProfile PlayerProfile { get; set; }
        public static Player Player { get; set; }
    }
}
