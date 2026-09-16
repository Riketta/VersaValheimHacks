using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
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

    /// <summary>
    /// Temporarily pulls user-marked items out of the source inventory so
    /// vanilla StackAll cannot move them, then puts the untouched stacks
    /// back into their original slots. Covers the vanilla chest button and
    /// the area-stack hotkey alike, since both end in this method. Vanilla
    /// syncing (Changed, RPCs) stays fully in place for the moved items.
    /// </summary>
    [HarmonyPatch(typeof(Inventory), "StackAll", new[] { typeof(Inventory), typeof(bool) })]
    internal class Inventory_StackAll
    {
        private static void Prefix(Inventory fromInventory, ref List<ItemDrop.ItemData> __state)
        {
            __state = null;
            if (fromInventory is null || !StackProtection.IsEnabled)
                return;

            List<ItemDrop.ItemData> pulled = null;
            foreach (ItemDrop.ItemData item in fromInventory.GetAllItems())
            {
                if (StackProtection.IsMarked(item))
                    (pulled ??= new List<ItemDrop.ItemData>()).Add(item);
            }

            if (pulled is null)
                return;

            foreach (ItemDrop.ItemData item in pulled)
                fromInventory.RemoveItem(item);

            __state = pulled;
        }

        private static void Postfix(Inventory fromInventory, List<ItemDrop.ItemData> __state)
        {
            if (__state is null || __state.Count == 0)
                return;

            // StackAll never writes into the source inventory, so every
            // marked stack's slot is still free - re-add it in place (the
            // grid position is preserved on the item itself).
            List<ItemDrop.ItemData> items = fromInventory.GetAllItems();
            foreach (ItemDrop.ItemData item in __state)
            {
                if (!items.Contains(item))
                    items.Add(item);
            }

            InventoryChangedMethod?.Invoke(fromInventory, new object[] { false, false });
            __state.Clear();
        }

        private static readonly MethodInfo InventoryChangedMethod =
            AccessTools.Method(typeof(Inventory), "Changed");
    }
}
