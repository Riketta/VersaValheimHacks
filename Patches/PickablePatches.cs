using HarmonyLib;
using VersaValheimHacks.Features;

namespace VersaValheimHacks.Patches
{
    [HarmonyPatch(typeof(Pickable), nameof(Pickable.Interact))]
    internal class Pickable_Interact
    {
        private static void Postfix(Pickable __instance, Humanoid character, bool repeat, bool alt)
            => AreaPickup.PickNearby(__instance, character, repeat, alt);
    }
}
