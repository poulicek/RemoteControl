using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace InputHook
{
    public static class HooksManager
    {
        #region P/Invoke

        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;        

        public enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
        }


        [StructLayout(LayoutKind.Sequential)]
        private struct KeyboardHookStruct
        {
            /// <summary>
            /// Specifies a virtual-key code. The code must be a value in the range 1 to 254. 
            /// </summary>
            public int VirtualKeyCode;
            /// <summary>
            /// Specifies a hardware scan code for the key. 
            /// </summary>
            public int ScanCode;
            /// <summary>
            /// Specifies the extended-key flag, event-injected flag, context code, and transition-state flag.
            /// </summary>
            public int Flags;
            /// <summary>
            /// Specifies the Time stamp for this message.
            /// </summary>
            public int Time;
            /// <summary>
            /// Specifies extra information associated with the message. 
            /// </summary>
            public int ExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern short GetKeyState(int vKey);

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        #endregion

        private static IntPtr mouseHookId = IntPtr.Zero;
        private static IntPtr keyboardHookId = IntPtr.Zero;

        private static Keys unhookKey;
        private static Keys unhookKeyModifier;


        public static void SetHooks(Keys unhookKey = Keys.None, Keys unhookKeyModifier = Keys.None)
        {
            HooksManager.unhookKey = unhookKey;
            HooksManager.unhookKeyModifier = unhookKeyModifier;

            mouseHookId = SetWindowsHookEx(WH_MOUSE_LL, mouseHookCallback, Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]), 0);
            keyboardHookId = SetWindowsHookEx(WH_KEYBOARD_LL, keyboardHookCallback, Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]), 0);
        }

        public static void UnHook()
        {
            UnhookWindowsHookEx(mouseHookId);
            UnhookWindowsHookEx(keyboardHookId);
        }

        private static IntPtr mouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            return new IntPtr(-1);   
        }


        private static IntPtr keyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            var key = getKey(lParam);
            if (isKeyModifier(key))
                return CallNextHookEx(keyboardHookId, nCode, wParam, lParam);

            if (key == unhookKey)
            {
                if (unhookKeyModifier == Keys.None || (GetKeyState((int)unhookKeyModifier) & 0x80) == 0x80)
                    UnHook();
            }

            return new IntPtr(-1);
        }

        private static Keys getKey(IntPtr ptr)
        {
            return (Keys)((KeyboardHookStruct)Marshal.PtrToStructure(ptr, typeof(KeyboardHookStruct))).VirtualKeyCode;
        }


        private static bool isKeyModifier(Keys key)
        {
            switch (key)
            {
                case Keys.LShiftKey:
                case Keys.RShiftKey:
                case Keys.RControlKey:
                case Keys.LControlKey:
                case Keys.Alt:
                    return true;
            }

            return false;
        }
    }
}
