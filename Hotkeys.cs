using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Playables;

namespace VersaValheimHacks
{
    internal class Hotkeys
    {
        public static void Init()
        {
            RegisterConfigReloadHotkeys();
            RegisterToggleHacksHotkeys();

            RegisterToggleDebugHotkeys();
            RegisterDumpHotkeys();
            if (GlobalState.Config.Debug)
                RegisterExtraDebugHotkeys();

            RegisterCustomNotificationHotkeys();
            RegisterRefreshFoodHotkeys();
            RegisterRevealWholeMapHotkeys();
        }

        static void RegisterConfigReloadHotkeys()
        {
            void configReloadHandler(WinApi.VirtualKeys _)
            {
                HarmonyLog.Log("Reloading config...");
                GlobalState.Config = Config.LoadOrCreateDefault(GlobalState.Config.PathToConfig);

                KeyManager.RemoveKeyPressedHandler(configReloadHandler);
                KeyManager.AddKeyPressedHandler(GlobalState.Config.HotkeysOptions.ReloadConfig, configReloadHandler);

                if (GlobalState.Config.Debug)
                    HarmonyLog.Log($"Current config:{Environment.NewLine}{GlobalState.Config.ToJson()}");

                NotificationManager.Notification($"Config reloaded!");
            };

            KeyManager.AddKeyPressedHandler(GlobalState.Config.HotkeysOptions.ReloadConfig, configReloadHandler);
        }

        static void RegisterToggleHacksHotkeys()
        {
            KeyManager.AddKeyPressedHandler(GlobalState.Config.HotkeysOptions.ToggleHacks, (_) =>
            {
                GlobalState.Config.Enabled = !GlobalState.Config.Enabled;
                NotificationManager.Notification($"Hacks state: {GlobalState.Config.Enabled}.", MessageHud.MessageType.TopLeft);
            });
        }

        static void RegisterToggleDebugHotkeys()
        {
            KeyManager.AddKeyPressedHandler(GlobalState.Config.HotkeysOptions.ToggleDebug, (_) =>
            {
                GlobalState.Config.Debug = !GlobalState.Config.Debug;
                NotificationManager.Notification($"Debug state: {GlobalState.Config.Debug}.", MessageHud.MessageType.TopLeft);
            });
        }

        static void RegisterDumpHotkeys()
        {
            KeyManager.AddKeyPressedHandler(GlobalState.Config.HotkeysOptions.DumpDebugLogs, (_) =>
            {
                if (GlobalState.Config.Debug)
                    DebugTools.DumpAll();
            });
        }

        static void RegisterExtraDebugHotkeys()
        {
            void printMessageNum4A(WinApi.VirtualKeys key) => HarmonyLog.Log($"[A] Key pressed: {key}; Expected: {WinApi.VirtualKeys.Numpad4}.");
            void printMessageNum4B(WinApi.VirtualKeys key) => HarmonyLog.Log($"[B] Key pressed: {key}; Expected: {WinApi.VirtualKeys.Numpad4}.");
            void printMessageNum5(WinApi.VirtualKeys key) => HarmonyLog.Log($"[!] Key pressed: {key}; Expected: {WinApi.VirtualKeys.Numpad5}.");
            void unregisterAllKeyEvents(WinApi.VirtualKeys key)
            {
                KeyManager.RemoveAllKeyPressedHandlers();
                HarmonyLog.Log($"All key events unregistered.");
            }

            KeyManager.AddKeyPressedHandler(WinApi.VirtualKeys.Numpad4, printMessageNum4A);
            KeyManager.AddKeyPressedHandler(WinApi.VirtualKeys.Numpad4, printMessageNum4B);
            KeyManager.AddKeyPressedHandler(WinApi.VirtualKeys.Numpad5, printMessageNum5);
            KeyManager.AddKeyPressedHandler(WinApi.VirtualKeys.Numpad6, unregisterAllKeyEvents);
        }

        static void RegisterCustomNotificationHotkeys()
        {
            KeyManager.AddKeyPressedHandler(
                GlobalState.Config.HotkeysOptions.SendCustomNotificationToNearbyPlayers, (_) =>
                NotificationManager.SendNotificationToNerabyPlayers(GlobalState.Config.NotificationOptions.CustomMessageToNearbyPlayers, GlobalState.Config.NotificationOptions.CustomMessageToNearbyPlayersRadius));
        }

        static void RegisterRefreshFoodHotkeys()
        {
            KeyManager.AddKeyPressedHandler(GlobalState.Config.HotkeysOptions.RefreshFood, (_) =>
            {
                if (GlobalState.Player is null)
                {
                    HarmonyLog.Log($"[{nameof(Hotkeys)}] Can't update food duration: no player instance saved!");
                    return;
                }

                HarmonyLog.Log($"[{nameof(Hotkeys)}] Trying to refresh food duration...");

                var foods = GlobalState.Player.GetFoods();
                foreach (var food in foods)
                {
                    HarmonyLog.Log($"[{nameof(Hotkeys)}] Updating food timer: {food.m_name} = {GlobalState.Config.BetterEatingOptions.FoodBuffDuration} (current: {food.m_time}).");
                    food.m_time = GlobalState.Config.BetterEatingOptions.FoodBuffDuration;
                }
            });
        }

        static void RegisterRevealWholeMapHotkeys()
        {
            KeyManager.AddKeyPressedHandler(GlobalState.Config.HotkeysOptions.RevealWholeMap, (_) =>
            {
                NotificationManager.Notification($"Trying to reveal whole map!");

                if (GlobalState.Player is null || Minimap.instance is null || !GlobalState.ToggleExtraHacks)
                    return;

                HarmonyLog.Log($"[{nameof(Hotkeys)}] Revealing whole map. Player: {GlobalState.Player.m_name}; Minimap: {Minimap.instance.name}.");
                NotificationManager.Notification($"Revealing whole map!");

                try
                {
                    FieldInfo m_exploredField = AccessTools.Field(typeof(Minimap), "m_explored");
                    bool[] m_explored = m_exploredField.GetValue(Minimap.instance) as bool[];

                    for (int i = 0; i < m_explored.Length; i++)
                        m_explored[i] = true;
                }
                catch (Exception ex)
                {
                    NotificationManager.Notification($"Exception: {ex}.");
                }
            });
        }
    }
}
