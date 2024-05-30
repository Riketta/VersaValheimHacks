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
                Config config = new Config();
                GlobalState.Config = config;

                HarmonyLog.Log("Trying to apply all patches...");
                Harmony harmony = new Harmony(Id);
                harmony.PatchAll();
                HarmonyLog.Log("All patches applied!");
            }
            catch (Exception ex)
            {
                HarmonyLog.Log($"Exception: {ex}.");
            }

            new Thread(() =>
            {
                Thread.Sleep(5000);
            }).Start();
        }
    }
}
