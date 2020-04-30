using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KeyboardLocker.Input
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
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelCallbackProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern short GetKeyState(int vKey);

        private delegate IntPtr LowLevelCallbackProc(int nCode, IntPtr wParam, IntPtr lParam);

        #endregion

        private static IntPtr mouseHookId = IntPtr.Zero;
        private static IntPtr keyboardHookId = IntPtr.Zero;

        private static KeyCombination triggerKey;
        private static LowLevelCallbackProc mouseCallback;
        private static LowLevelCallbackProc keyboardCallback;

        public static event Action InputBlocked;
        public static event Action KeyCombinationTriggered;

        public static bool BlockInput { get; private set; }


        /// <summary>
        /// Re-sets the hooks for keyboard and mouse
        /// </summary>
        public static void SetHooks(KeyCombination trigger)
        {
            SetHooks(trigger, BlockInput);
        }


        /// <summary>
        /// Sets the hooks for keyboard and mouse (depending on if input blocking is requested)
        /// </summary>
        public static void SetHooks(KeyCombination trigger, bool blockInput)
        {
            UnHook();

            HooksManager.BlockInput = blockInput;
            HooksManager.triggerKey = trigger;
            
            // callbacks have their instance variables to prevent their destrcution by garbage collector
            keyboardHookId = SetWindowsHookEx(WH_KEYBOARD_LL, keyboardCallback = new LowLevelCallbackProc(keyboardHookCallback), Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]), 0);            
            if (blockInput)
                mouseHookId = SetWindowsHookEx(WH_MOUSE_LL, mouseCallback = new LowLevelCallbackProc(mouseBlockingHookCallback), Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]), 0);
        }


        /// <summary>
        /// Clears the hooks
        /// </summary>
        public static void UnHook()
        {
            if (mouseHookId != IntPtr.Zero)
                UnhookWindowsHookEx(mouseHookId);

            if (keyboardHookId != IntPtr.Zero)
                UnhookWindowsHookEx(keyboardHookId);

            mouseHookId = IntPtr.Zero;
            keyboardHookId = IntPtr.Zero;

            mouseCallback = null;
            keyboardCallback = null;
        }


        /// <summary>
        /// Process the mouse events and always blocks them
        /// </summary>
        private static IntPtr mouseBlockingHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (wParam == (IntPtr)MouseMessages.WM_LBUTTONDOWN || wParam == (IntPtr)MouseMessages.WM_RBUTTONDOWN)
                    InputBlocked?.Invoke();                
            }
            catch { }

            return new IntPtr(-1);
        }


        /// <summary>
        /// Processes the keyboard events and blocks them if blocking is on
        /// </summary>
        private static IntPtr keyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                var key = getKey(lParam);
                var isDown = wParam == (IntPtr)WM_KEYDOWN;

                // detection of the trigger key
                if (isDown && isTriggerKey(key))
                    KeyCombinationTriggered?.Invoke();

                // propagate event if input is not blocked
                if (!BlockInput)
                    return CallNextHookEx(keyboardHookId, nCode, wParam, lParam);

                // modifiers are always allowed
                if (isKeyAllowed(key))
                    return CallNextHookEx(keyboardHookId, nCode, wParam, lParam);

                // inform about the blocked key
                if (isDown)
                    InputBlocked?.Invoke();
            }
            catch { }

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
            if (triggerKey == null)
                return false;

            // checking the main key
            if (triggerKey.MainKey != key)
                return false;

            // checking if all modifiers are pressed
            foreach (var modifierKey in triggerKey.Modifiers)
                if ((GetKeyState((int)modifierKey) & 0x80) != 0x80)
                    return false;

            return true;
        }

        
        /// <summary>
        /// Returns true if the key is a modifier
        /// </summary>
        private static bool isKeyAllowed(Keys key)
        {
            if (triggerKey != null)
                return false;

            foreach (var modifierKey in triggerKey.Modifiers)
            {
                if (modifierKey == key)
                    return true;

                if (modifierKey == Keys.ControlKey)
                    return key == Keys.LControlKey || key == Keys.RControlKey;

                if (modifierKey == Keys.ShiftKey)
                    return key == Keys.LShiftKey || key == Keys.RShiftKey;
            }

            return false;
        }

        #endregion
    }
}
