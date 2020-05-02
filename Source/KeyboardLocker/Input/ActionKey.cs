using System;
using System.Windows.Forms;

namespace KeyboardLocker.Input
{
    public class ActionKey
    {
        private const int LONG_PRESS_TIMEOUT = 600;

        private readonly Timer longPressTimer = new Timer() { Interval = LONG_PRESS_TIMEOUT };

        public static implicit operator ActionKey(Keys k) => new ActionKey(k);

        public event Func<bool> Pressed;
        public event Func<bool> Released;
        public event Action LongPress;

        public readonly Keys Key;

        public bool IsPressed { get; private set; }
        public bool WasLongPressed { get; private set; }


        public ActionKey(Keys key)
        {
            this.Key = key;
            this.longPressTimer.Tick += this.longPressCallback;
        }

        public bool SetPressed(bool pressed)
        {
            this.IsPressed = false;
            this.longPressTimer.Stop();

            // key release
            if (pressed)
            {
                this.IsPressed = true;
                this.WasLongPressed = false;
                this.longPressTimer.Start();
                
                return this.Pressed?.Invoke() == true;
            }
            
            return this.Released?.Invoke() == true;
        }

        private void longPressCallback(object sender, EventArgs e)
        {
            this.longPressTimer.Stop();
            this.WasLongPressed = this.IsPressed;
            this.LongPress?.Invoke();
        }

        public override string ToString()
        {
            return this.Key.ToString();
        }
    }
}
