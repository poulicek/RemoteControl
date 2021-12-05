using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TrayToolkit.Helpers;

namespace RemoteControl.Controllers.Grip
{
    public class GripButtonKeys : IGripButton
    {
        private readonly System.Windows.Forms.Keys[] keys;
        private readonly List<Keys> pressedKeys = new List<Keys>();

        public bool ScanMode { get; set; }


        public GripButtonKeys(Keys[] keys)
        {
            this.keys = keys;
        }


        /// <summary>
        /// Releases the button
        /// </summary>
        public void Release()
        {
            this.pressKeys(new Keys[0]);
        }


        /// <summary>
        /// Presses the keys according to the coordinates
        /// </summary>
        public void Press(PointF coords)
        {
            this.pressKeys(this.getPressedKeys(this.keys, coords));
        }


        /// <summary>
        /// Returns the pressed keys
        /// </summary>
        private Keys[] getPressedKeys(Keys[] keys, PointF coords)
        {
            var x = coords.X;
            var y = coords.Y;

            var slope = x == 0 ? double.PositiveInfinity : Math.Abs(y / x);
            var isDiagonal = slope > 0.5 && slope < 2;

            if (x == 0 && y == 0)
                return new Keys[0];

            if (x >= 0 && y >= 0)
                return isDiagonal
                    ? new Keys[] { keys[0], keys[1] }
                    : new Keys[] { slope > 1 ? keys[0] : keys[1] };

            if (x >= 0 && y <= 0)
                return isDiagonal
                    ? new Keys[] { keys[1], keys[2] }
                    : new Keys[] { slope > 1 ? keys[2] : keys[1] };

            if (x <= 0 && y <= 0)
                return isDiagonal
                    ? new Keys[] { keys[2], keys[3] }
                    : new Keys[] { slope > 1 ? keys[2] : keys[3] };


            if (x <= 0 && y >= 0)
                return isDiagonal
                    ? new Keys[] { keys[3], keys[0] }
                    : new Keys[] { slope > 1 ? keys[0] : keys[3] };

            return new Keys[0];
        }


        /// <summary>
        /// Presses the keys
        /// </summary>
        private void pressKeys(Keys[] keys)
        {
            lock (this.pressedKeys)
            {
                var unpressKeys = this.pressedKeys.Where(k => !keys.Contains(k)).ToArray();
                var pressKeys = keys.Where(k => !this.pressedKeys.Contains(k)).ToArray();

                foreach (var key in unpressKeys)
                {
                    key.Up(this.ScanMode);
                    this.pressedKeys.Remove(key);
                }

                foreach (var key in pressKeys)
                {
                    key.Down(this.ScanMode);
                    this.pressedKeys.Add(key);
                }
            }
        }
    }
}
