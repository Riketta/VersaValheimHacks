using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaValheimHacks.Patches
{
    //[HarmonyPatch(typeof(Player), "AddStamina")]
    internal class AddStamina
    {
        private const string Prefix = "Player.AddStamina";

        [HarmonyPrefix]
        public static void StaminaGainMultiplier(ref float v)
        {
            HarmonyLog.Log($"[{Prefix}.Prefix] Stamina gain (real): {v}.");
            v *= 2f;
        }
    }
}
