using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VersaValheimHacks.Features
{
    /// <summary>

    /// Adds a "Sort" button below the container panel's "Place stacks"

    /// button (a clone of it, slimmed to fit the word). Clicking sorts the
    /// open container alphabetically by localized item name and repacks the

    /// grid row-major from the top-left. Items carry their own slot
    /// coordinates (m_gridPos), so both the list order and the slot positions
    /// are rewritten; the game's own Changed pipeline then saves and syncs
    /// the chest exactly like the vanilla StackAll button.

    /// </summary>
    internal static class ContainerSort
    {
        private const string Prefix = "ContainerSort";
        private const string ButtonName = "SortButton";

        private static readonly FieldInfo CurrentContainerField = AccessTools.Field(typeof(InventoryGui), "m_currentContainer");
        private static readonly FieldInfo InventoryListField = AccessTools.Field(typeof(Inventory), "m_inventory");
        private static readonly MethodInfo InventoryChangedMethod = AccessTools.Method(typeof(Inventory), "Changed");
        private static readonly MethodInfo SetupDragItemMethod = AccessTools.Method(typeof(InventoryGui), "SetupDragItem");

        internal static bool IsEnabled => GlobalState.Config.ContainerOptions.SortButton;

        /// <summary>Awake postfix: clones the Stack All button once per GUI instance.</summary>
        public static void EnsureButton(InventoryGui gui)
        {
            try
            {
                if (!IsEnabled || gui == null || gui.m_stackAllButton == null)
                    return;

                Transform parent = gui.m_stackAllButton.transform.parent;
                if (parent == null || parent.Find(ButtonName) != null)
                    return; // already created for this GUI instance

                GameObject sortGo = UnityEngine.Object.Instantiate(gui.m_stackAllButton.gameObject, parent);
                sortGo.name = ButtonName;

                Button button = sortGo.GetComponent<Button>();

                // Replace (not RemoveAllListeners) so any persistent inspector
                // listeners cloned from the original are dropped too.
                button.onClick = new Button.ButtonClickedEvent();

                button.onClick.AddListener(SortOpenContainer);



                SetLabel(sortGo, "Sort");

                // Below the Stack All button (a layout group, if any, overrides this).

                if (sortGo.transform is RectTransform rect && gui.m_stackAllButton.transform is RectTransform stackRect)

                {
                    rect.anchoredPosition = stackRect.anchoredPosition - new Vector2(0f, stackRect.rect.height + 8f);

                    // Slim the button so only the word fits.

                    rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, LabelWidth(sortGo, rect.rect.height));

                    // Nudge it right by half its own width.

                    rect.anchoredPosition += new Vector2(rect.rect.width * 0.5f, 0f);

                }

                HarmonyLog.Log($"[{Prefix}] Sort button added to the container panel.");
            }
            catch (Exception ex)
            {
                HarmonyLog.Log($"[{Prefix}] EnsureButton exception: {ex}.");
            }
        }

        public static void SortOpenContainer()
        {
            try
            {
                Player player = Player.m_localPlayer;
                InventoryGui gui = InventoryGui.instance;
                if (gui == null || player == null || player.IsTeleporting())
                    return;

                if (!(CurrentContainerField?.GetValue(gui) is Container container) || container == null)
                    return;

                Inventory inventory = container.GetInventory();
                if (inventory == null)
                    return;

                if (!(InventoryListField?.GetValue(inventory) is List<ItemDrop.ItemData> items) || items.Count < 2)
                    return;

                // Drop any item being dragged out of this chest first (as the

                // vanilla Stack All button does).

                SetupDragItemMethod?.Invoke(gui, new object[] { null, null, 1 });



                items.Sort(CompareItems);


                // The grid places every item at its own m_gridPos coordinate,
                // so reordering the list alone changes nothing on screen:
                // assign slots row-major, top-left first (this also packs gaps).
                int width = inventory.GetWidth();
                for (int i = 0; i < items.Count; i++)
                    items[i].m_gridPos = new Vector2i(i % width, i / width);

                InventoryChangedMethod?.Invoke(inventory, new object[] { false, false });

                HarmonyLog.Log($"[{Prefix}] Container sorted ({items.Count} stacks).");
            }
            catch (Exception ex)
            {
                HarmonyLog.Log($"[{Prefix}] SortOpenContainer exception: {ex}.");
            }
        }

        private static int CompareItems(ItemDrop.ItemData a, ItemDrop.ItemData b)
        {
            int result = string.CompareOrdinal(LocalizedName(a), LocalizedName(b));
            if (result != 0)
                return result;

            // List.Sort is unstable - pin ties so equal names keep a
            // deterministic order.
            result = string.CompareOrdinal(a?.m_dropPrefab?.name, b?.m_dropPrefab?.name);
            if (result != 0)
                return result;

            result = (a?.m_quality ?? 0).CompareTo(b?.m_quality ?? 0);
            if (result != 0)
                return result;

            return (a?.m_variant ?? 0).CompareTo(b?.m_variant ?? 0);
        }

        private static string LocalizedName(ItemDrop.ItemData item)
        {
            string name = item?.m_shared?.m_name;
            return Localization.instance != null && name != null
                ? Localization.instance.Localize(name)
                : name ?? "";
        }

        private static void SetLabel(GameObject buttonGo, string text)

        {

            TMP_Text tmpLabel = buttonGo.GetComponentInChildren<TMP_Text>(true);

            if (tmpLabel != null)

                tmpLabel.text = text;



            Text legacyLabel = buttonGo.GetComponentInChildren<Text>(true);

            if (legacyLabel != null)

                legacyLabel.text = text;

        }


        /// <summary>Width that makes the button fit only its word.</summary>
        private static float LabelWidth(GameObject buttonGo, float fallback)
        {
            TMP_Text tmpLabel = buttonGo.GetComponentInChildren<TMP_Text>(true);
            if (tmpLabel != null)
                return Mathf.Max(tmpLabel.preferredWidth + 30f, 60f);

            Text legacyLabel = buttonGo.GetComponentInChildren<Text>(true);
            if (legacyLabel != null)
                return Mathf.Max(legacyLabel.preferredWidth + 30f, 60f);

            return fallback;
        }

    }
}
