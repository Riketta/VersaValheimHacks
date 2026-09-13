using System;
using System.Collections.Concurrent;

namespace VersaValheimHacks
{
    /// <summary>
    /// Unity APIs (textures, UI, game objects) may only be used from the main
    /// thread. Hotkeys are polled on a background thread, so their handlers are
    /// queued here and drained every frame by MainThreadPatches.
    /// </summary>
    internal static class MainThread
    {
        private static readonly ConcurrentQueue<Action> _queue = new ConcurrentQueue<Action>();

        public static void Run(Action action) => _queue.Enqueue(action);

        public static void Drain()
        {
            while (_queue.TryDequeue(out var action))
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    HarmonyLog.Log($"[MainThread] Exception: {ex}.");
                    NotificationManager.Notification($"Exception: {ex}.");
                }
            }
        }
    }
}
