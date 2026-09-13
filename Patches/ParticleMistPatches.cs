using HarmonyLib;
using VersaValheimHacks.Features;

namespace VersaValheimHacks.Patches
{
    [HarmonyPatch(typeof(ParticleMist), "Update")]
    internal class ParticleMist_Update
    {
        private static bool Prefix() => NoMist.AllowUpdate;
    }
}
