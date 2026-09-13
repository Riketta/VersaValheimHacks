using HarmonyLib;
using System;
using System.Threading;

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
                GlobalState.Config = Config.LoadOrCreateDefault(Config.DefaultConfigPath);

                HarmonyLog.Log("Applying all patches...");
                Harmony harmony = new Harmony(Id);
                harmony.PatchAll();
                HarmonyLog.Log("All patches applied!");

                HarmonyLog.Log("Registering hotkeys...");
                Hotkeys.Init();

                // The handle must be captured on the main thread; the polling thread owns no windows.
                GlobalState.GameWindowHandle = WindowsManager.GetCurrentThreadWindowHandle();
            }
            catch (Exception ex)
            {
                FileLog.Log($"[{DateTime.Now:HH:mm:ss.fffffff}] Exception: {ex}.");
            }

            var keyPollingThread = new Thread(KeyPollingLoop)
            {
                IsBackground = true, // don't keep the game process alive on exit
                Name = nameof(VersaValheimHacks) + "." + nameof(KeyPollingLoop),
            };
            keyPollingThread.Start();
        }

        private static void KeyPollingLoop()
        {
            while (true)
            {
                if (WindowsManager.IsWindowInFocus(GlobalState.GameWindowHandle))
                    KeyManager.KeyPollingIteration();

                Thread.Sleep(5);
            }
        }
    }
}
