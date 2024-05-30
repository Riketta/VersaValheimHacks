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

                HarmonyLog.Log("Trying to apply all patches...");
                Harmony harmony = new Harmony(Id);
                harmony.PatchAll();
                HarmonyLog.Log("All patches applied!");

                HarmonyLog.Log("Registring hotkeys...");
                Hotkeys.Init();
            }
            catch (Exception ex)
            {
                FileLog.Log($"[{DateTime.Now:HH:mm:ss.fffffff}] Exception: {ex}.");
            }

            new Thread(() =>
            {
                while (true)
                {
                    if (WindowsManager.IsWindowInFocus(GlobalState.GameWindowHandle))
                        KeyManager.KeyPollingIteration();

                    Thread.Sleep(5);
                }
            }).Start();
        }
    }
}
