using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace KeyboardLocker.Input
{
    public class KeyCombination
    {
        public static implicit operator KeyCombination(Keys k) => new KeyCombination(k);

        public event Func<bool> Pressed;
        public event Func<bool> Released;

        public Keys Key;

        public bool IsEmpty { get { return this.Key == Keys.None; } }


        public KeyCombination(Keys key)
        {
            this.Key = key;
        }

        public bool SetPressed(bool pressed)
        {
            return pressed
                ? this.Pressed?.Invoke() == true
                : this.Released?.Invoke() == true;
        }

        public override string ToString()
        {
            return this.Key.ToString();
        }
    }
}
