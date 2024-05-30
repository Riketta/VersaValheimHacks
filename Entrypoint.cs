using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VersaValheimHacks
{
    internal class Entrypoint
    {
        public static readonly string Id = $"Riketta.{nameof(VersaValheimHacks)}";

        public static void Init()
        {
            try
            {
                HarmonyLog.Log("Reading config...");
                var configPath = Config.DefaultConfigPath;
                Config config = Config.LoadOrCreateDefault(configPath);
                GlobalState.Config = config;
                KeyManager.AddKeyPressedHandler(config.ConfigReloadHotkey, (_) => // TODO: re-register config reload handle on config change.
                {
                    HarmonyLog.Log("Reloading config...");
                    GlobalState.Config = Config.LoadOrCreateDefault(configPath);

                    if (GlobalState.Config.Debug)
                        HarmonyLog.Log($"Hacks enabled: {GlobalState.Config.Enabled}.");
                });

                HarmonyLog.Log("Trying to apply all patches...");
                Harmony harmony = new Harmony(Id);
                harmony.PatchAll();
                HarmonyLog.Log("All patches applied!");

                if (config.Debug)
                {
                    void printMessageNum4(WinApi.VirtualKeys key) => HarmonyLog.Log($"Key pressed: {key}; Expected: {WinApi.VirtualKeys.Numpad4}.");
                    void printMessageNum5(WinApi.VirtualKeys key) => HarmonyLog.Log($"Key pressed: {key}; Expected: {WinApi.VirtualKeys.Numpad5}.");
                    void unregisterAllKeyEvents(WinApi.VirtualKeys key)
                    {
                        HarmonyLog.Log($"All key events unregistered.");
                        KeyManager.RemoveKeyPressedHandler(printMessageNum4);
                        KeyManager.RemoveKeyPressedHandler(printMessageNum5);
                    }

                    KeyManager.AddKeyPressedHandler(WinApi.VirtualKeys.Numpad4, printMessageNum4);
                    KeyManager.AddKeyPressedHandler(WinApi.VirtualKeys.Numpad5, printMessageNum5);
                    KeyManager.AddKeyPressedHandler(WinApi.VirtualKeys.Numpad6, unregisterAllKeyEvents);
                }


            }
            catch (Exception ex)
            {
                FileLog.Log($"[{DateTime.Now:HH:mm:ss.fffffff}] Exception: {ex}.");
            }

            new Thread(() =>
            {
                while (true)
                {
                    KeyManager.KeyPollingIteration();
                    Thread.Sleep(5);
                }
            }).Start();
        }
    }
}
