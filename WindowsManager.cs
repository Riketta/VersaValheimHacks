using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaValheimHacks
{
    internal class WindowsManager
    {
        static Dictionary<WinApi.VirtualKeys, bool> KeyboardState = new Dictionary<WinApi.VirtualKeys, bool>();

        public static bool IsCapsLockOn => (WinApi.GetKeyState(WinApi.VirtualKeys.CapsLock) & 1) > 0;

        public static bool IsKeyDown(WinApi.VirtualKeys key)
        {
            return (1 & (WinApi.GetKeyState(key) >> 15)) == 1;
        }

        public static bool IsKeyPressed(WinApi.VirtualKeys key)
        {
            bool currentState = IsKeyDown(key);
            if (!KeyboardState.ContainsKey(key))
            {
                KeyboardState[key] = currentState;
                return currentState;
            }

            bool previousState = KeyboardState[key];
            KeyboardState[key] = currentState;

            return currentState && !previousState;
        }

        public static bool IsKeyReleased(WinApi.VirtualKeys key)
        {
            bool currentState = IsKeyDown(key);
            if (!KeyboardState.ContainsKey(key))
            {
                KeyboardState[key] = currentState;
                return !currentState;
            }

            bool previousState = KeyboardState[key];
            KeyboardState[key] = currentState;

            return !currentState && previousState;
        }

        public static bool IsWindowInFocus(IntPtr handle)
        {
            return handle == WinApi.GetForegroundWindow();
        }
    }
}
