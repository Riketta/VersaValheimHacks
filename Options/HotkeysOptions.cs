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
        public WinApi.VirtualKeys StreamerMode { get; set; } = WinApi.VirtualKeys.End;
        public WinApi.VirtualKeys ToggleDebug { get; set; } = WinApi.VirtualKeys.Numpad7;
        /// <summary>
        /// Dump global keys/values and window handles to the log
        /// (debug mode required).
        /// </summary>
        public WinApi.VirtualKeys DumpDebugInfo { get; set; } = WinApi.VirtualKeys.Numpad8;
        public WinApi.VirtualKeys DumpGameObjects { get; set; } = WinApi.VirtualKeys.Numpad9;
        /// <summary>
        /// Dump the whole ObjectDB (items, recipes with ingredients, unique
        /// ingredient set) to VersaValheimHacks.ItemDump.txt in the game root
        /// (debug mode required). Unbound by default - Numpad6 now toggles
        /// stack protection; set a key here if you need the dump again.
        /// </summary>
        public WinApi.VirtualKeys DumpItemDatabase { get; set; } = WinApi.VirtualKeys.None;
        /// <summary>
        /// Toggle stack protection on the item under the mouse pointer
        /// (inventory or container panel): marked items are skipped by
        /// every stack-to-chest operation and show a colored outline.
        /// </summary>
        public WinApi.VirtualKeys ToggleStackProtection { get; set; } = WinApi.VirtualKeys.Numpad6;

        public WinApi.VirtualKeys SendCustomNotificationToNearbyPlayers { get; set; } = WinApi.VirtualKeys.None;
        public WinApi.VirtualKeys RevealWholeMap { get; set; } = WinApi.VirtualKeys.None;
        public WinApi.VirtualKeys ApplySavedFood { get; set; } = WinApi.VirtualKeys.Numpad1;
        public WinApi.VirtualKeys SaveFood { get; set; } = WinApi.VirtualKeys.Numpad2;
        public WinApi.VirtualKeys ClearFood { get; set; } = WinApi.VirtualKeys.Numpad3;
        public WinApi.VirtualKeys ApplyRested { get; set; } = WinApi.VirtualKeys.Numpad4;
        public WinApi.VirtualKeys StackToNearbyChests { get; set; } = WinApi.VirtualKeys.Numpad5;
    }
}
