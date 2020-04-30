using System.Collections.Generic;
using System.Windows.Forms;

namespace KeyboardLocker.Input
{
    public class KeyCombination
    {
        public static implicit operator KeyCombination(Keys k) => new KeyCombination(k);

        public Keys MainKey;
        public readonly List<Keys> Modifiers = new List<Keys>();

        public bool IsEmpty { get { return this.MainKey == Keys.None; } }


        public KeyCombination(Keys mainKey, params Keys[] modifiers)
        {
            this.MainKey = mainKey;
            if (modifiers != null)
                this.Modifiers.AddRange(modifiers);
        }

        public override string ToString()
        {
            var str = this.MainKey.ToString();
            foreach (var m in this.Modifiers)
                str += " + " + m.ToString();

            return str;
        }
    }
}
