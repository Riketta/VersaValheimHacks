using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static VersaValheimHacks.KeyManager;

namespace VersaValheimHacks
{
    class KeyPressedEvent
    {
        event KeyPressedDelegate _keyPressedEvent;

        public void Invoke(WinApi.VirtualKeys key)
        {
            if (GlobalState.Config.Debug)
                HarmonyLog.Log($"Invoke {key}.");
            _keyPressedEvent?.Invoke(key);
        }

        public bool Contains(KeyPressedDelegate handler)
        {
            return _keyPressedEvent.GetInvocationList().Contains(handler);
        }

        public void AddHandler(KeyPressedDelegate keyPressedDelegate)
        {
            _keyPressedEvent += keyPressedDelegate;
        }

        public void RemoveHandler(KeyPressedDelegate keyPressedDelegate)
        {
            _keyPressedEvent -= keyPressedDelegate;
        }
    }

    internal class KeyManager
    {
        public delegate void KeyPressedDelegate(WinApi.VirtualKeys key);

        /// <summary>
        /// Per key events for key pressed events.
        /// </summary>
        static readonly Dictionary<WinApi.VirtualKeys, KeyPressedEvent> _keysPressedHandlers = new Dictionary<WinApi.VirtualKeys, KeyPressedEvent>();

        /// <summary>
        /// Register handler as event for specific key.
        /// </summary>
        /// <param name="key">Key that will invoke handler as event on pressed.</param>
        /// <param name="handler">Handler to register as event.</param>
        public static void AddKeyPressedHandler(WinApi.VirtualKeys key, KeyPressedDelegate handler)
        {
            if (!_keysPressedHandlers.ContainsKey(key))
                _keysPressedHandlers[key] = new KeyPressedEvent();
            
            _keysPressedHandlers[key].AddHandler(handler);
        }

        /// <summary>
        /// Remove handler from specific key.
        /// </summary>
        /// <param name="key">Key to remove handler from.</param>
        /// <param name="handler">Handler to unregister.</param>
        public static void RemoveKeyPressedHandler(WinApi.VirtualKeys key, KeyPressedDelegate handler)
        {
            if (_keysPressedHandlers.ContainsKey(key))
                _keysPressedHandlers[key].RemoveHandler(handler);
        }

        /// <summary>
        /// Remove handler from all keys.
        /// </summary>
        /// <param name="handler">Handler to unregister.</param>
        public static void RemoveKeyPressedHandler(KeyPressedDelegate handler)
        {
            foreach (var key in _keysPressedHandlers.Keys)
            {
                if (GlobalState.Config.Debug)
                    HarmonyLog.Log($"Removing handler from {key}.");

                _keysPressedHandlers[key].RemoveHandler(handler);
            }
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
