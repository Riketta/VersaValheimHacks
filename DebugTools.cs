using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Player;

namespace VersaValheimHacks
{
    internal class DebugTools
    {
        private const string Prefix = "DEBUG";

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

                    Component[] components = ValheimUtils.GetAllComponents(gameObject);
                    foreach (var component in components)
                        HarmonyLog.Log($"[{Prefix}] > {component}.");
                }
                catch (Exception ex)
                {
                    HarmonyLog.Log($"[{Prefix}] Object: {gameObject.name}; Exception: {ex}.");
                }
            }

            NotificationManager.Notification($"Dumped {validObjectsCount} object(s) (total: {gameObjects.Length}).");
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

            FieldInfo m_globalKeysField = AccessTools.Field(typeof(ZoneSystem), "m_globalKeys.");
            HashSet<string> m_globalKeys = m_globalKeysField.GetValue(GlobalState.ZoneSystem) as HashSet<string>;
            HarmonyLog.Log("## m_globalKeys.");
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

        public static void OnZoneSystemInstantiated(ZoneSystem zoneSystem)
        {
            HarmonyLog.Log("[+] OnZoneSystemInstantiated.");
            GlobalState.ZoneSystem = zoneSystem;
            DumpZoneSystem();
            DumpWorld();
        }

        public static void OnWorldInstantiated(World world)
        {
            HarmonyLog.Log("[+] OnWorldInstantiated.");
            GlobalState.World = world;
            DumpWorld();
            DumpZoneSystem();
        }

        internal static void OnCrouching()
        {
            HarmonyLog.Log("[+] OnCrouching.");
            GlobalState.ZoneSystem = ZoneSystem.instance;
        }
    }
}
