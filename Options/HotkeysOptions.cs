using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaValheimHacks.Options
{
    internal class HotkeysOptions
    {
        public WinApi.VirtualKeys ReloadConfig { get; set; } = WinApi.VirtualKeys.Home;
        public WinApi.VirtualKeys ToggleHacks { get; set; } = WinApi.VirtualKeys.Numpad0;
        public WinApi.VirtualKeys ToggleDebug { get; set; } = WinApi.VirtualKeys.Numpad7;
        public WinApi.VirtualKeys DumpDebugLogs { get; set; } = WinApi.VirtualKeys.Numpad8;
        public WinApi.VirtualKeys DumpGameObjects { get; set; } = WinApi.VirtualKeys.Numpad9;

        public WinApi.VirtualKeys SendCustomNotificationToNearbyPlayers { get; set; } = WinApi.VirtualKeys.Numpad3;
        public WinApi.VirtualKeys RevealWholeMap { get; set; } = WinApi.VirtualKeys.Numpad2;
        public WinApi.VirtualKeys RefreshFood { get; set; } = WinApi.VirtualKeys.Numpad1;
    }
}
