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
        public static void IsCrouching(bool crouch)
        {
            HarmonyLog.Log($"[{Prefix}.Postfix] Player crouching: {crouch}.");
            GlobalState.IsPlayerCrouching = crouch;
        }
    }
}
