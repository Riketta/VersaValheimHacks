using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                HarmonyLog.Log($"VersaValheimHacks v{typeof(Entrypoint).Assembly.GetName().Version}: reading config...");
                GlobalState.Config = Config.LoadOrCreateDefault(Config.DefaultConfigPath);

                HarmonyLog.Log("Applying patches...");
                Harmony harmony = new Harmony(Id);
                ApplyPatchesSafely(harmony);

                HarmonyLog.Log("Registering hotkeys...");
                Hotkeys.Init();

                // The handle must be captured on the main thread; the polling thread owns no windows.
                GlobalState.GameWindowHandle = WindowsManager.GetCurrentThreadWindowHandle();
            }
            catch (Exception ex)
            {
                HarmonyLog.Log($"[Entrypoint] Init failed: {ex}");
            }

            var keyPollingThread = new Thread(KeyPollingLoop)
            {
                IsBackground = true, // don't keep the game process alive on exit
                Name = nameof(VersaValheimHacks) + "." + nameof(KeyPollingLoop),
            };
            keyPollingThread.Start();
        }

        /// <summary>
        /// Applies every [HarmonyPatch] class independently: one broken patch
        /// (game update renamed a target, missing field) can never take down
        /// the rest of the mod or the hotkeys.
        /// </summary>
        private static void ApplyPatchesSafely(Harmony harmony)
        {
            int applied = 0, failed = 0;
            foreach (var patchType in GetPatchTypes())
            {
                try
                {
                    harmony.CreateClassProcessor(patchType).Patch();
                    applied++;
                    HarmonyLog.Log($"[Entrypoint] Patched {patchType.FullName}.");
                }
                catch (Exception ex)
                {
                    failed++;
                    HarmonyLog.Log($"[Entrypoint] PATCH FAILED: {patchType.FullName}: {ex}");
                }
            }

            HarmonyLog.Log($"[Entrypoint] Patches applied: {applied}, failed: {failed}.");
        }

        private static IEnumerable<Type> GetPatchTypes()
        {
            Type[] types;
            try
            {
                types = typeof(Entrypoint).Assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types;
            }

            return types
                .Where(t => t is { IsClass: true } && t.GetCustomAttribute<HarmonyPatch>() != null)
                .OrderBy(t => t.FullName, StringComparer.Ordinal);
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
