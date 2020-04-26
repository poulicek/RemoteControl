using System.Collections.Generic;
using System.Windows.Forms;

namespace InputHookWin
{
    public class KeyCombination
    {
        public Keys MainKey;
        public readonly List<Keys> Modifiers = new List<Keys>();

        public bool IsEmpty { get { return this.MainKey == Keys.None; } }


        public KeyCombination(Keys mainKey)
        {
            this.MainKey = mainKey;
        }
    }
}
