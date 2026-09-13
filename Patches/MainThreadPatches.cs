using HarmonyLib;

namespace VersaValheimHacks.Patches
{
    /// <summary>
    /// Per-frame hooks that execute queued main-thread work (hotkey handlers).
    /// </summary>
    internal static class MainThreadPatches
    {
        [HarmonyPatch(typeof(Player), "Update")]
        internal class Player_Update
        {
            private static void Postfix() => MainThread.Drain();
        }

        [HarmonyPatch(typeof(FejdStartup), "Update")]
        internal class FejdStartup_Update
        {
            private static void Postfix() => MainThread.Drain();
        }
    }
}
