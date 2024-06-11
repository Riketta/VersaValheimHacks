using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaValheimHacks.Patches
{
    internal class NoMist
    {
        [HarmonyPatch(typeof(ParticleMist), "Update")]
        internal class ParticleMistUpdate
        {
            private const string Prefix = "ParticleMist.Update";

            [HarmonyPrefix]
            public static bool DisableMist()
            {
                if (!GlobalState.ToggleHacks || !GlobalState.Config.GodModeOptions.DisableMistlandsMist)
                    return true;

                return false;
            }
        }
    }
}
