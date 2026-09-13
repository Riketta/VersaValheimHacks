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

        /// <summary>
        /// Local player instance, captured by SetCrouch patch.
        /// </summary>
        public static Player Player { get; set; }

        public static ZoneSystem ZoneSystem { get; set; }

        public static World World { get; set; }

        public static bool IsPlayerCrouching { get; set; }

        /// <summary>
        /// Game window handle, captured once on the main thread in Entrypoint.Init.
        /// </summary>
        public static IntPtr GameWindowHandle { get; set; }

        /// <summary>
        /// Master switch for main hacks.
        /// </summary>
        public static bool ToggleHacks => Config?.Enabled == true;

        /// <summary>
        /// Master switch for extra (cheatier) hacks: recipe unlocking, map reveal.
        /// </summary>
        public static bool ToggleExtraHacks => Config?.Debug == true;

        /// <summary>
        /// Master switch for debug tools and instance capture patches.
        /// </summary>
        public static bool EnableDebugTools => Config?.Debug == true;
    }
}
