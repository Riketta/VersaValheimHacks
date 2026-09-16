using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace VersaValheimHacks.Features
{
    /// <summary>
    /// Per-item "keep with me" marking: items tagged through the vanilla
    /// m_customData dictionary are skipped by every StackAll operation -
    /// the vanilla chest button and the Numpad5 area stack alike. The tag
    /// is stored in the game's own item custom data, so it persists in the
    /// save file and syncs in multiplayer. Marked slots show a colored
    /// outline around the icon while an inventory panel is open.
    /// </summary>
    internal static class StackProtection
    {
        private const string Prefix = "StackProtection";
        private const string MarkKey = "versa_stackProtect";
        private const string MarkValue = "1";
        private const string DefaultMarkColorHex = "#FF5A5A";

        private static readonly FieldInfo GridInventoryField =
            AccessTools.Field(typeof(InventoryGrid), "m_inventory");

        private static readonly FieldInfo GridElementsField =
            AccessTools.Field(typeof(InventoryGrid), "m_elements");

        private static readonly MethodInfo GetHoveredElementMethod =
            AccessTools.Method(typeof(InventoryGrid), "GetHoveredElement");

        private static readonly MethodInfo InventoryChangedMethod =
            AccessTools.Method(typeof(Inventory), "Changed");

        internal static bool IsEnabled => GlobalState.Config.AreaStackOptions.ProtectMarkedItems;

        public static bool IsMarked(ItemDrop.ItemData item)
        {
            return item != null
                && item.m_customData != null
                && item.m_customData.TryGetValue(MarkKey, out string value)
                && string.Equals(value, MarkValue, StringComparison.Ordinal);
        }

        /// <summary>
        /// Toggles the mark on the item under the mouse pointer (Numpad6).
        /// Works on both the player inventory and an open container panel.
        /// </summary>
        public static void ToggleHovered()
        {
            try
            {
                if (!IsEnabled)
                {
                    NotificationManager.Notification("Stack protection is disabled in config.", MessageHud.MessageType.TopLeft);
                    return;
                }

                InventoryGui gui = InventoryGui.instance;
                if (gui is null)
                    return;

                foreach (InventoryGrid grid in new[] { gui.m_playerGrid, gui.ContainerGrid })
                {
                    if (grid is null || !grid.gameObject.activeInHierarchy)
                        continue;

                    if (!(GetHoveredElementMethod?.Invoke(grid, null) is InventoryElement element))
                        continue;

                    Inventory inventory = GridInventoryField?.GetValue(grid) as Inventory;
                    ItemDrop.ItemData item = inventory?.GetItemAt(element.Position.x, element.Position.y);
                    if (item is null)
                    {
                        NotificationManager.Notification("Empty slot - hover an item to toggle stack protection.", MessageHud.MessageType.TopLeft);
                        return;
                    }

                    bool marked = !IsMarked(item);
                    SetMarked(item, marked);
                    NotificationManager.Notification(
                        $"Stack protection {(marked ? "ON" : "OFF")}: {LocalizedName(item)}.",
                        MessageHud.MessageType.TopLeft);
                    return;
                }

                // Nothing hovered - stay silent to avoid spamming outside the inventory.
            }
            catch (Exception ex)
            {
                HarmonyLog.Log($"[{Prefix}] ToggleHovered exception: {ex}.");
            }
        }

        /// <summary>
        /// Refreshes the outline marker on every slot of one grid. Runs as
        /// an UpdateGui postfix, so it follows adds, moves and stack merges
        /// without extra bookkeeping.
        /// </summary>
        public static void UpdateMarks(InventoryGrid grid)
        {
            if (grid is null)
                return;

            try
            {
                Inventory inventory = GridInventoryField?.GetValue(grid) as Inventory;
                if (!(GridElementsField?.GetValue(grid) is IEnumerable<InventoryElement> elements) || inventory is null)
                    return;

                bool enabled = IsEnabled;
                Color color = ParseColor(GlobalState.Config.AreaStackOptions.MarkedColor);
                foreach (InventoryElement element in elements)
                {
                    if (element is null || element.m_icon is null)
                        continue;

                    ItemDrop.ItemData item = inventory.GetItemAt(element.Position.x, element.Position.y);
                    ApplyOutline(element.m_icon, enabled && IsMarked(item), color);
                }
            }
            catch (Exception ex)
            {
                HarmonyLog.Log($"[{Prefix}] UpdateMarks exception: {ex}.");
            }
        }

        private static void SetMarked(ItemDrop.ItemData item, bool marked)
        {
            if (marked)
                item.m_customData[MarkKey] = MarkValue;
            else
                item.m_customData.Remove(MarkKey);
        }

        private static void ApplyOutline(Image icon, bool marked, Color color)
        {
            Outline outline = icon.GetComponent<Outline>();
            if (outline is null)
            {
                if (!marked)
                    return;

                outline = icon.gameObject.AddComponent<Outline>();
                outline.effectDistance = new Vector2(2.5f, -2.5f);
            }

            outline.effectColor = color;
            outline.enabled = marked;
        }

        private static Color ParseColor(string hex)
        {
            string value = (hex ?? string.Empty).Trim();
            if (!value.StartsWith("#"))
                value = "#" + value;

            if (ColorUtility.TryParseHtmlString(value, out Color color))
                return color;

            ColorUtility.TryParseHtmlString(DefaultMarkColorHex, out color);
            return color;
        }

        private static string LocalizedName(ItemDrop.ItemData item)
        {
            string token = item.m_shared.m_name;
            Localization localization = Localization.instance;
            return localization != null ? localization.Localize(token) : token;
        }
    }
}
