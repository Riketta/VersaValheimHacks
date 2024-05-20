using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VersaValheimHacks
{
    internal class WinApi
    {
        [DllImport("USER32.dll")] public static extern short GetKeyState(int nVirtKey);
        public static bool IsCapsLockOn => (GetKeyState(0x14) & 1) > 0;
    }
}
