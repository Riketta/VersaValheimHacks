using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaValheimHacks.Patches
{
    [HarmonyPatch(typeof(Player), "SetCrouch")]
    internal class SetCrouch
    {
        private const string Prefix = "Player.SetCrouch";

        [HarmonyPostfix]
        public static void IsCrouching(Player __instance, bool crouch)
        {
            if (__instance != null)
                GlobalState.Player = __instance; // TODO: move to dedicated Player constructor patch.

            HarmonyLog.Log($"[{Prefix}.Postfix] Player crouching: {crouch}.");
            GlobalState.IsPlayerCrouching = crouch;

            if (crouch)
                DebugTools.OnCrouching();
        }
    }
}
