using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using static VersaValheimHacks.KeyManager;

namespace VersaValheimHacks
{
    class KeyPressedEvent
    {
        event KeyPressedDelegate Event;

        public void Invoke(WinApi.VirtualKeys key)
        {
            if (GlobalState.Config.Debug)
                HarmonyLog.Log($"[{nameof(KeyPressedEvent)}] Invoke {Event?.GetInvocationList()?.Length ?? 0} handler(s) for the {key}.");

            Event?.Invoke(key);
        }

        public bool Contains(KeyPressedDelegate handler)
        {
            if (Event is null || Event.GetInvocationList() is null)
                return false;

            return Event.GetInvocationList().Contains(handler);
        }

        public void AddHandler(KeyPressedDelegate keyPressedDelegate)
        {
            Event += keyPressedDelegate;
        }

        public void RemoveHandler(KeyPressedDelegate keyPressedDelegate)
        {
            Event -= keyPressedDelegate;
        }

        public void RemoveAllHandlers()
        {
            foreach (var handler in Event.GetInvocationList())
                RemoveHandler(handler as KeyPressedDelegate);
        }
    }

    internal class KeyManager
    {
        public delegate void KeyPressedDelegate(WinApi.VirtualKeys key);

        /// <summary>
        /// Per key events for key pressed events.
        /// </summary>
        static readonly ConcurrentDictionary<WinApi.VirtualKeys, KeyPressedEvent> _keysPressedHandlers = new ConcurrentDictionary<WinApi.VirtualKeys, KeyPressedEvent>();

        /// <summary>
        /// Register handler as event for specific key.
        /// </summary>
        /// <param name="key">Key that will invoke handler as event on pressed.</param>
        /// <param name="handler">Handler to register as event.</param>
        public static void AddKeyPressedHandler(WinApi.VirtualKeys key, KeyPressedDelegate handler)
        {
            if (!_keysPressedHandlers.ContainsKey(key))
                _keysPressedHandlers[key] = new KeyPressedEvent();

            if (GlobalState.Config.Debug)
                HarmonyLog.Log($"[{nameof(KeyPressedEvent)}] Adding handler for the {key}.");

            _keysPressedHandlers[key].AddHandler(handler);
        }

        /// <summary>
        /// Remove handler for specific key.
        /// </summary>
        /// <param name="key">Key to remove handler from.</param>
        /// <param name="handler">Handler to unregister.</param>
        public static void RemoveKeyPressedHandler(WinApi.VirtualKeys key, KeyPressedDelegate handler)
        {
            if (_keysPressedHandlers.ContainsKey(key))
            {
                if (GlobalState.Config.Debug)
                    HarmonyLog.Log($"[{nameof(KeyPressedEvent)}] Removing handler from the {key} (if present).");

                _keysPressedHandlers[key].RemoveHandler(handler);
            }
        }

        /// <summary>
        /// Remove handler for all keys.
        /// </summary>
        /// <param name="handler">Handler to unregister.</param>
        public static void RemoveKeyPressedHandler(KeyPressedDelegate handler)
        {
            foreach (var key in _keysPressedHandlers.Keys)
                RemoveKeyPressedHandler(key, handler);
        }

        /// <summary>
        /// Remove all handlers for all keys.
        /// </summary>
        public static void RemoveAllKeyPressedHandlers()
        {
            if (GlobalState.Config.Debug)
                HarmonyLog.Log($"[{nameof(KeyPressedEvent)}] Removing all handlers for all keys.");

            foreach (var key in _keysPressedHandlers.Keys)
                _keysPressedHandlers[key].RemoveAllHandlers();
        }

        public static void KeyPollingIteration()
        {
            try
            {
                foreach (var kvPair in _keysPressedHandlers)
                    if (WindowsManager.IsKeyPressed(kvPair.Key))
                        kvPair.Value.Invoke(kvPair.Key);
            }
            catch (Exception ex)
            {
                HarmonyLog.Log($"[{nameof(KeyManager)}] Exception: {ex}.");
            }
        }
    }
}
