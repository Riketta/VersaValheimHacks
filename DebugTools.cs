using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace VersaValheimHacks
{
    internal class DebugTools
    {
        private const string Prefix = "DEBUG";

        private const string ItemDumpFileName = "VersaValheimHacks.ItemDump.txt";

        public static void DumpAllItemsAroundPlayer()
        {
            if (GlobalState.Player is null)
                return;

            var gameObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            int validObjectsCount = 0;
            foreach (var gameObject in gameObjects)
            {
                try
                {
                    float distance = gameObject?.transform?.position.DistanceTo(GlobalState.Player.transform.position) ?? float.MaxValue;
                    bool isInRange = distance < 5f;

                    if (!isInRange || distance < 0.001f)
                        continue;

                    validObjectsCount++;

                    HarmonyLog.Log($"[{Prefix}] Object: {gameObject.name}; Distance: {distance:F2}.");

                    foreach (var component in gameObject.GetComponents(typeof(Component)))
                        HarmonyLog.Log($"[{Prefix}] > {component}.");
                }
                catch (Exception ex)
                {
                    HarmonyLog.Log($"[{Prefix}] Object: {gameObject.name}; Exception: {ex}.");
                }
            }

            NotificationManager.Notification($"Dumped {validObjectsCount} object(s) (total: {gameObjects.Length}).");
        }

        /// <summary>
        /// Writes the whole ObjectDB (every item, every recipe with its
        /// ingredients, and the unique ingredient set) to ItemDumpFileName
        /// in the game root. Used to keep CraftingSort.IngredientRegion
        /// complete when the game updates its item pool.
        /// </summary>
        public static void DumpItemDatabase()
        {
            try
            {
                ObjectDB objectDb = ObjectDB.instance;
                if (objectDb is null)
                {
                    NotificationManager.Notification("Item dump failed: ObjectDB not ready (enter a world first).", MessageHud.MessageType.TopLeft);
                    return;
                }

                Localization localization = Localization.instance;
                var dump = new StringBuilder();

                dump.AppendLine($"# Items ({objectDb.m_items.Count}). Format: prefab|localized name|type|description.");
                int itemCount = 0;
                foreach (var item in objectDb.m_items)
                {
                    if (item == null)
                        continue;

                    var drop = item.GetComponent<ItemDrop>();
                    if (drop == null || drop.m_itemData == null || drop.m_itemData.m_shared == null)
                        continue;

                    string nameToken = drop.m_itemData.m_shared.m_name;
                    string descriptionToken = drop.m_itemData.m_shared.m_description;
                    string localizedName = localization != null ? localization.Localize(nameToken) : nameToken;
                    string localizedDescription = localization != null ? localization.Localize(descriptionToken) : descriptionToken;
                    dump.AppendLine($"{item.name}|{localizedName}|{drop.m_itemData.m_shared.m_itemType}|{localizedDescription}");
                    itemCount++;
                }

                var ingredients = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
                dump.AppendLine();
                dump.AppendLine($"# Recipes ({objectDb.m_recipes.Count}). Format: recipe|output|station level|ingredients (prefab:amount).");
                int recipeCount = 0;
                foreach (var recipe in objectDb.m_recipes)
                {
                    if (recipe is null)
                        continue;

                    string output = recipe.m_item != null ? recipe.m_item.name : "<null>";
                    var parts = new List<string>();
                    if (recipe.m_resources != null)
                    {
                        foreach (var requirement in recipe.m_resources)
                        {
                            if (requirement?.m_resItem is null)
                                continue;

                            parts.Add($"{requirement.m_resItem.name}:{requirement.m_amount}");
                            ingredients.Add(requirement.m_resItem.name);
                        }
                    }

                    dump.AppendLine($"{recipe.name}|{output}|{recipe.m_minStationLevel}|{string.Join(", ", parts)}");
                    recipeCount++;
                }

                dump.AppendLine();
                dump.AppendLine($"# Unique ingredients ({ingredients.Count}) - the names CraftingSort.IngredientRegion must cover.");
                foreach (string ingredient in ingredients)
                    dump.AppendLine(ingredient);

                string path = Path.GetFullPath(ItemDumpFileName);
                File.WriteAllText(path, dump.ToString());

                HarmonyLog.Log($"[{Prefix}] Item database dumped to {path} ({itemCount} items, {recipeCount} recipes, {ingredients.Count} unique ingredients).");
                NotificationManager.Notification($"Item DB dumped: {itemCount} items, {recipeCount} recipes, {ingredients.Count} ingredients.", MessageHud.MessageType.TopLeft);
            }
            catch (Exception ex)
            {
                HarmonyLog.Log($"[{Prefix}] DumpItemDatabase exception: {ex}.");
                NotificationManager.Notification("Item dump failed (see log).", MessageHud.MessageType.TopLeft);
            }
        }

        public static void DumpAll()
        {
            HarmonyLog.Log($"[{Prefix}] Dump All.");

            DumpZoneSystem();
            DumpWorld();

            HarmonyLog.Log($"[{Prefix}] Process.GetCurrentProcess().MainWindowHandle: {Process.GetCurrentProcess().MainWindowHandle}.");
            HarmonyLog.Log($"[{Prefix}] WindowsManager.GetCurrentThreadWindowHandle(): {WindowsManager.GetCurrentThreadWindowHandle()}.");
            HarmonyLog.Log($"[{Prefix}] GlobalState.GameWindowHandle: {GlobalState.GameWindowHandle}.");
        }

        public static void DumpZoneSystem()
        {
            HarmonyLog.Log("# ZoneSystem.");
            if (GlobalState.ZoneSystem is null)
            {
                HarmonyLog.Log("ZoneSystem is null!");
                return;
            }

            FieldInfo m_globalKeysField = AccessTools.Field(typeof(ZoneSystem), "m_globalKeys");
            HashSet<string> m_globalKeys = m_globalKeysField.GetValue(GlobalState.ZoneSystem) as HashSet<string>;
            HarmonyLog.Log("## m_globalKeys.");
            if (m_globalKeys == null)
            {
                HarmonyLog.Log("<null or missing>.");
                return;
            }

            foreach (string key in m_globalKeys)
                HarmonyLog.Log($"> {key}.");

            HarmonyLog.Log("## m_globalKeysValues.");
            foreach (var key in GlobalState.ZoneSystem.m_globalKeysValues)
                HarmonyLog.Log($"> {key.Key} = {key.Value}.");

            HarmonyLog.Log("## m_globalKeysEnums.");
            foreach (var key in GlobalState.ZoneSystem.m_globalKeysEnums)
                HarmonyLog.Log($"> [{(int)key}] {key}.");
        }

        public static void DumpWorld()
        {
            HarmonyLog.Log("# World.");
            if (GlobalState.World is null)
            {
                HarmonyLog.Log("World is null!");
                return;
            }

            HarmonyLog.Log("## m_startingGlobalKeys.");
            foreach (string key in GlobalState.World.m_startingGlobalKeys)
                HarmonyLog.Log($"> {key}.");
        }

        public static void RevealWholeMap()
        {
            if (GlobalState.Player is null || Minimap.instance is null)
                return;

            if (!GlobalState.ToggleExtraHacks)
            {
                NotificationManager.Notification("Map reveal requires debug mode.", MessageHud.MessageType.TopLeft);
                return;
            }

            HarmonyLog.Log($"[{Prefix}] Revealing whole map. Player: {GlobalState.Player.m_name}; Minimap: {Minimap.instance.name}.");
            Minimap.instance.ExploreAll();
            NotificationManager.Notification("Whole map revealed.", MessageHud.MessageType.TopLeft);
        }

        public static void OnZoneSystemInstantiated(ZoneSystem zoneSystem)
        {
            try
            {
                if (!GlobalState.EnableDebugTools)
                    return;

                HarmonyLog.Log("[+] OnZoneSystemInstantiated.");
                GlobalState.ZoneSystem = zoneSystem;
                DumpZoneSystem();
                DumpWorld();
            }
            catch (Exception ex)
            {
                // A debug dump must never break world initialization.
                HarmonyLog.Log($"[DebugTools] Exception: {ex}.");
            }
        }

        public static void OnWorldInstantiated(World world)
        {
            try
            {
                if (!GlobalState.EnableDebugTools)
                    return;

                HarmonyLog.Log("[+] OnWorldInstantiated.");
                GlobalState.World = world;
                DumpWorld();
                DumpZoneSystem();
            }
            catch (Exception ex)
            {
                // A debug dump must never break world initialization.
                HarmonyLog.Log($"[DebugTools] Exception: {ex}.");
            }
        }

        internal static void OnCrouching()
        {
            if (!GlobalState.EnableDebugTools)
                return;

            HarmonyLog.Log("[+] OnCrouching.");
            GlobalState.ZoneSystem = ZoneSystem.instance;
        }
    }
}
