using HarmonyLib;
using VersaValheimHacks.Features;

namespace VersaValheimHacks.Patches
{
    /// <summary>
    /// Full AddItem overload used by crafting/upgrading - the short overload
    /// delegates into it, so one prefix covers both. Strips the cheated tag.
    /// </summary>
    [HarmonyPatch(typeof(Inventory), "AddItem", new[]
    {
        typeof(string), typeof(int), typeof(int), typeof(int),
        typeof(long), typeof(string), typeof(Vector2i), typeof(bool), typeof(bool), typeof(bool)
    })]
    internal class Inventory_AddItem
    {
        private static void Prefix(ref bool cheated) => FreeCrafting.RemoveCheatedTag(ref cheated);
    }
}
