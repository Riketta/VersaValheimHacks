using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Player;

namespace VersaValheimHacks
{
    internal class DebugTools
    {
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
            DumpWorld();
            GlobalState.ZoneSystem = ZoneSystem.instance;
            DumpZoneSystem();
        }
    }
}
