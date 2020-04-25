using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

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
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        #endregion

        private static IntPtr mouseHookId = IntPtr.Zero;
        private static IntPtr keyboardHookId = IntPtr.Zero;
        private static IntPtr unhookKey = IntPtr.Zero;


        public static void SetHooks()
        {
            SetHooks(IntPtr.Zero);
        }

        public static void SetHooks(IntPtr unhookKey)
        {
            HooksManager.unhookKey = unhookKey;
            mouseHookId = SetWindowsHookEx(WH_MOUSE_LL, hookCallback, Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]), 0);
            keyboardHookId = SetWindowsHookEx(WH_KEYBOARD_LL, hookCallback, Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]), 0);
        }

        public static void UnHook()
        {
            UnhookWindowsHookEx(mouseHookId);
            UnhookWindowsHookEx(keyboardHookId);
        }

        private static IntPtr hookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && unhookKey != IntPtr.Zero && wParam == unhookKey)
                UnHook();

            return new IntPtr(-1);   
        }
    }
}
