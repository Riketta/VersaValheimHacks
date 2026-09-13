using System;

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
            RegisterDumpGameObjects();
            if (GlobalState.Config.Debug)
                RegisterExtraDebugHotkeys();

            RegisterCustomNotificationHotkeys();
            RegisterCycleFoodHotkeys();
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
                NotificationManager.Notification(GlobalState.Config.Enabled ? "Godmode enabled." : "Godmode disabled.", MessageHud.MessageType.TopLeft);
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

        static void RegisterDumpGameObjects()
        {
            KeyManager.AddKeyPressedHandler(GlobalState.Config.HotkeysOptions.DumpGameObjects, (_) =>
            {
                if (GlobalState.Config.Debug)
                    DebugTools.DumpAllItemsAroundPlayer();
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
                NotificationManager.SendToNearbyPlayers(GlobalState.Config.NotificationOptions.CustomMessageToNearbyPlayers, GlobalState.Config.NotificationOptions.CustomMessageToNearbyPlayersRadius));
        }

        static void RegisterCycleFoodHotkeys()
        {
            KeyManager.AddKeyPressedHandler(GlobalState.Config.HotkeysOptions.RefreshFood, (_) => Features.BetterEating.CycleNow());
        }

        static void RegisterRevealWholeMapHotkeys()
        {
            KeyManager.AddKeyPressedHandler(GlobalState.Config.HotkeysOptions.RevealWholeMap, (_) =>
            {
                NotificationManager.Notification($"Trying to reveal whole map!");
                DebugTools.RevealWholeMap();
            });
        }
    }
}
