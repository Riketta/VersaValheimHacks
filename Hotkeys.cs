using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaValheimHacks
{
    internal class Hotkeys
    {
        public static void Init()
        {
            RegisterConfigReloadHotkeys();
            RegisterToggleHacksHotkeys();

            RegisterToggleDebugHotkeys();
            RegisterDebugDumpHotkeys();
            if (GlobalState.Config.Debug)
                RegisterExtraDebugHotkeys();
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
            };

            KeyManager.AddKeyPressedHandler(GlobalState.Config.HotkeysOptions.ReloadConfig, configReloadHandler);
        }

        static void RegisterToggleHacksHotkeys()
        {
            KeyManager.AddKeyPressedHandler(GlobalState.Config.HotkeysOptions.ToggleHacks, (_) => GlobalState.Config.Enabled = !GlobalState.Config.Enabled);
        }

        static void RegisterToggleDebugHotkeys()
        {
            KeyManager.AddKeyPressedHandler(GlobalState.Config.HotkeysOptions.ToggleDebug, (_) => GlobalState.Config.Debug = !GlobalState.Config.Debug);
        }

        static void RegisterDebugDumpHotkeys()
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
                KeyManager.RemoveKeyPressedHandler(printMessageNum4A);
                KeyManager.RemoveKeyPressedHandler(printMessageNum4B);
                KeyManager.RemoveKeyPressedHandler(printMessageNum5);
                HarmonyLog.Log($"All key events unregistered.");
            }

            KeyManager.AddKeyPressedHandler(WinApi.VirtualKeys.Numpad4, printMessageNum4A);
            KeyManager.AddKeyPressedHandler(WinApi.VirtualKeys.Numpad4, printMessageNum4B);
            KeyManager.AddKeyPressedHandler(WinApi.VirtualKeys.Numpad5, printMessageNum5);
            KeyManager.AddKeyPressedHandler(WinApi.VirtualKeys.Numpad6, unregisterAllKeyEvents);
        }
    }
}
