using System;

namespace VersaValheimHacks
{
    internal class Hotkeys
    {
        public static void Init()
        {
            RegisterConfigReloadHotkeys();
            RegisterToggleHacksHotkeys();
            RegisterStreamerModeHotkeys();

            RegisterToggleDebugHotkeys();
            RegisterDumpHotkeys();
            RegisterDumpGameObjects();
            RegisterDumpItemDatabaseHotkeys();

            RegisterCustomNotificationHotkeys();
            RegisterApplySavedFoodHotkeys();
            RegisterSaveFoodHotkeys();
            RegisterClearFoodHotkeys();
            RegisterRevealWholeMapHotkeys();
            RegisterAreaStackHotkeys();
            RegisterApplyRestedHotkeys();
            RegisterStackProtectionHotkeys();
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

                NotificationManager.Notification("Config reloaded.");
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
                NotificationManager.Notification(GlobalState.Config.Debug ? "Debug mode enabled." : "Debug mode disabled.", MessageHud.MessageType.TopLeft);
            });
        }

        static void RegisterStreamerModeHotkeys()
        {
            KeyManager.AddKeyPressedHandler(GlobalState.Config.HotkeysOptions.StreamerMode, (_) =>
            {
                GlobalState.Config.StreamerMode = !GlobalState.Config.StreamerMode;
                GlobalState.Config.Save();

                if (GlobalState.Config.StreamerMode)
                    Features.BetterEating.CycleNow(); // snap already-extended foods to natural timers

                // forced: the toggle confirmation must show even in streamer mode
                NotificationManager.Notification(
                    GlobalState.Config.StreamerMode
                        ? "Streamer mode ON: vanilla-looking buffs, mod messages hidden."
                        : "Streamer mode off.",
                    MessageHud.MessageType.TopLeft,
                    force: true);
            });
        }

        static void RegisterDumpHotkeys()
        {
            KeyManager.AddKeyPressedHandler(GlobalState.Config.HotkeysOptions.DumpDebugInfo, (_) =>
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

        static void RegisterDumpItemDatabaseHotkeys()
        {
            KeyManager.AddKeyPressedHandler(GlobalState.Config.HotkeysOptions.DumpItemDatabase, (_) =>
            {
                if (GlobalState.Config.Debug)
                    DebugTools.DumpItemDatabase();
            });
        }

        static void RegisterCustomNotificationHotkeys()
        {
            KeyManager.AddKeyPressedHandler(
                GlobalState.Config.HotkeysOptions.SendCustomNotificationToNearbyPlayers, (_) =>
                NotificationManager.SendToNearbyPlayers(GlobalState.Config.NotificationOptions.CustomMessageToNearbyPlayers, GlobalState.Config.NotificationOptions.CustomMessageToNearbyPlayersRadius));
        }

        static void RegisterClearFoodHotkeys()
        {
            KeyManager.AddKeyPressedHandler(GlobalState.Config.HotkeysOptions.ClearFood, (_) => Features.BetterEating.ClearFoodNow());
        }

        static void RegisterApplySavedFoodHotkeys()
        {
            KeyManager.AddKeyPressedHandler(GlobalState.Config.HotkeysOptions.ApplySavedFood, (_) => Features.BetterEating.ApplySavedFood());
        }

        static void RegisterSaveFoodHotkeys()
        {
            KeyManager.AddKeyPressedHandler(GlobalState.Config.HotkeysOptions.SaveFood, (_) => Features.BetterEating.SaveCurrentFood());
        }

        static void RegisterRevealWholeMapHotkeys()
        {
            KeyManager.AddKeyPressedHandler(GlobalState.Config.HotkeysOptions.RevealWholeMap, (_) => DebugTools.RevealWholeMap());
        }

        static void RegisterAreaStackHotkeys()
        {
            KeyManager.AddKeyPressedHandler(GlobalState.Config.HotkeysOptions.StackToNearbyChests, (_) => Features.AreaStack.StackToNearbyChests());
        }

        static void RegisterApplyRestedHotkeys()
        {
            KeyManager.AddKeyPressedHandler(GlobalState.Config.HotkeysOptions.ApplyRested, (_) => Features.RestedBuff.ApplyToPlayer());
        }

        static void RegisterStackProtectionHotkeys()
        {
            KeyManager.AddKeyPressedHandler(GlobalState.Config.HotkeysOptions.ToggleStackProtection, (_) => Features.StackProtection.ToggleHovered());
        }
    }
}
