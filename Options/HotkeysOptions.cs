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
        /// <summary>
        /// Toggle verbose logging mode.
        /// </summary>
        public WinApi.VirtualKeys ToggleDebug { get; set; } = WinApi.VirtualKeys.Multiply;
        /// <summary>
        /// Dump global keys/values and window handles to the log
        /// (debug mode required). Unbound by default.
        /// </summary>
        public WinApi.VirtualKeys DumpDebugInfo { get; set; } = WinApi.VirtualKeys.None;
        /// <summary>
        /// Mark the top-left corner of the auto-plant field: the nearest
        /// planted crop.
        /// </summary>
        public WinApi.VirtualKeys AutoPlantMarkFirst { get; set; } = WinApi.VirtualKeys.Numpad7;
        /// <summary>
        /// Mark the bottom-left corner of the auto-plant field.
        /// </summary>
        public WinApi.VirtualKeys AutoPlantMarkSecond { get; set; } = WinApi.VirtualKeys.Numpad8;
        /// <summary>
        /// Mark the bottom-right corner of the auto-plant field and
        /// auto-plant the rectangle, one seed per plant.
        /// </summary>
        public WinApi.VirtualKeys AutoPlantMarkThird { get; set; } = WinApi.VirtualKeys.Numpad9;
        public WinApi.VirtualKeys DumpGameObjects { get; set; } = WinApi.VirtualKeys.None;
        /// <summary>
        /// Dump the whole ObjectDB (items, recipes with ingredients, unique
        /// ingredient set) to VersaValheimHacks.ItemDump.txt in the game root
        /// (debug mode required). Unbound by default; set a key here if you
        /// need the dump again.
        /// </summary>
        public WinApi.VirtualKeys DumpItemDatabase { get; set; } = WinApi.VirtualKeys.None;
        /// <summary>
        /// Toggle stack protection on the item under the mouse pointer
        /// (inventory or container panel): marked items are skipped by
        /// every stack-to-chest operation and show a colored outline.
        /// </summary>
        public WinApi.VirtualKeys ToggleStackProtection { get; set; } = WinApi.VirtualKeys.Numpad6;
        /// <summary>

        /// Despawn every friendly skeleton the local player summoned.

        /// Only own summons are touched (follow-target + player ID check).

        /// </summary>

        public WinApi.VirtualKeys DespawnSkeletons { get; set; } = WinApi.VirtualKeys.Subtract;
        /// <summary>

        /// Command your summoned skeletons to attack the creature under the

        /// crosshair at the moment of the key press; pressed with nothing in
        /// the crosshair, recall them to follow. When the target dies or

        /// disappears, skeletons fall back to their default follow behaviour.

        /// </summary>

        public WinApi.VirtualKeys CommandSkeletons { get; set; } = WinApi.VirtualKeys.Add;

        public WinApi.VirtualKeys SendCustomNotificationToNearbyPlayers { get; set; } = WinApi.VirtualKeys.None;
        public WinApi.VirtualKeys RevealWholeMap { get; set; } = WinApi.VirtualKeys.None;
        public WinApi.VirtualKeys ApplySavedFood { get; set; } = WinApi.VirtualKeys.Numpad1;
        public WinApi.VirtualKeys SaveFood { get; set; } = WinApi.VirtualKeys.Numpad2;
        public WinApi.VirtualKeys ClearFood { get; set; } = WinApi.VirtualKeys.Numpad3;
        /// <summary>

        /// Apply the rested buff on demand with the game's real current rest
        /// value (comfort-based).

        /// </summary>

        public WinApi.VirtualKeys ApplyRested { get; set; } = WinApi.VirtualKeys.Numpad4;
        public WinApi.VirtualKeys StackToNearbyChests { get; set; } = WinApi.VirtualKeys.Numpad5;
        /// <summary>
        /// Unlock every enabled crafting recipe on demand (debug mode
        /// required). Re-presses are harmless no-ops.
        /// </summary>
        public WinApi.VirtualKeys UnlockAllRecipes { get; set; } = WinApi.VirtualKeys.Divide;
    }
}
