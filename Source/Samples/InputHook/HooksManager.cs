using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using InputHookWin;

namespace InputHook
{
    public static class HooksManager
    {
        #region P/Invoke

        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;
        private const int WM_KEYDOWN = 256;
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

        private static KeyCombination triggerKey;
        
        public static event Action KeyCombinationTriggered;

        public static bool InputBlocked { get; private set; }


        /// <summary>
        /// Re-sets the hooks for keyboard and mouse
        /// </summary>
        public static void SetHooks(KeyCombination trigger)
        {
            SetHooks(trigger, InputBlocked);
        }


        /// <summary>
        /// Sets the hooks for keyboard and mouse (depending on if input blocking is requested)
        /// </summary>
        public static void SetHooks(KeyCombination trigger, bool blockInput)
        {
            UnHook();

            HooksManager.InputBlocked = blockInput;
            HooksManager.triggerKey = trigger;
            
            keyboardHookId = SetWindowsHookEx(WH_KEYBOARD_LL, keyboardHookCallback, Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]), 0);            
            if (blockInput)
                mouseHookId = SetWindowsHookEx(WH_MOUSE_LL, mouseBlockingHookCallback, Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]), 0);
        }


        /// <summary>
        /// Clears the hooks
        /// </summary>
        public static void UnHook()
        {
            UnhookWindowsHookEx(mouseHookId);
            UnhookWindowsHookEx(keyboardHookId);
        }


        /// <summary>
        /// Process the mouse events and always blocks them
        /// </summary>
        private static IntPtr mouseBlockingHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            return new IntPtr(-1);
        }


        /// <summary>
        /// Processes the keyboard events and blocks them if blocking is on
        /// </summary>
        private static IntPtr keyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            var key = getKey(lParam);

            // detection of the trigger key
            if ((int)wParam == WM_KEYDOWN && isTriggerKey(key) && KeyCombinationTriggered != null)
                KeyCombinationTriggered.Invoke();

            // modifiers are always allowed
            else if (!InputBlocked || isKeyModifier(key))
                return CallNextHookEx(keyboardHookId, nCode, wParam, lParam);

            // blocking the input;
            return new IntPtr(-1);
        }

        #region Helpers

        /// <summary>
        /// Converts the pointer into structure
        /// </summary>
        private static Keys getKey(IntPtr ptr)
        {
            return (Keys)((KeyboardHookStruct)Marshal.PtrToStructure(ptr, typeof(KeyboardHookStruct))).VirtualKeyCode;
        }


        /// <summary>
        /// Returns true if the pressed key is the trigger
        /// </summary>
        private static bool isTriggerKey(Keys key)
        {
            // checking the main key
            if (triggerKey.MainKey != key)
                return false;

            // checking if all modifiers are pressed
            if (triggerKey.Modifiers != null)
                foreach (var modifierKey in triggerKey.Modifiers)
                    if ((GetKeyState((int)modifierKey) & 0x80) != 0x80)
                        return false;

            return true;
        }

        
        /// <summary>
        /// Returns true if the key is a modifier
        /// </summary>
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

        #endregion
    }
}
