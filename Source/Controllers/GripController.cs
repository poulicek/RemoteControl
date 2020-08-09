using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using RemoteControl.Server;
using TrayToolkit.Helpers;

namespace RemoteControl.Controllers
{
    public class GripController : IController
    {
        private readonly List<Keys> pressedKeys = new List<Keys>();

        public void ProcessRequest(HttpContext context)
        {
            if (int.TryParse(context.Request.Query["s"], out var keyState) && keyState == 0)
                this.pressKeys();
            else
                this.pressKeys(this.parsePressedKeys(context.Request.Query["v"].Split(','), context.Request.Query["o"].Split(',')));
                
        }


        /// <summary>
        /// Parses the pressed keys
        /// </summary>
        private Keys[] parsePressedKeys(string[] keyCodes, string[] coords)
        {
            if (keyCodes.Length != 4 || coords.Length != 2 ||
                !double.TryParse(coords[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var x) ||
                !double.TryParse(coords[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var y))
                return null;

            return this.getPressedKeys(this.parseKeys(keyCodes), x, y);
        }


        /// <summary>
        /// Returns the pressed keys
        /// </summary>
        private Keys[] getPressedKeys(Keys[] keys, double x, double y)
        {
            var slope = x == 0 ? double.PositiveInfinity : Math.Abs(y / x);
            var isDiagonal = slope > 0.5 && slope < 2;

            if (x == 0 && y == 0)
                return null;

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

            return null;
        }


        /// <summary>
        /// Parses the keys codes
        /// </summary>
        private Keys[] parseKeys(string[] keyCodes)
        {
            var keys = new Keys[keyCodes.Length];

            for (int i = 0; i < keyCodes.Length; i++)
                if (int.TryParse(keyCodes[i], out var keyCode))
                    keys[i] = (Keys)keyCode;

            return keys;
        }


        /// <summary>
        /// Presses the keys
        /// </summary>
        private void pressKeys(params Keys[] keys)
        {
            if (keys == null)
                keys = new Keys[0];

            lock (this.pressedKeys)
            {
                var unpressKeys = this.pressedKeys.Where(k => !keys.Contains(k)).ToArray();
                var pressKeys = keys.Where(k => !this.pressedKeys.Contains(k)).ToArray();

                foreach (var key in unpressKeys)
                {
                    key.Up();
                    this.pressedKeys.Remove(key);
                }

                foreach (var key in pressKeys)
                {
                    key.Down();
                    this.pressedKeys.Add(key);
                }

                //BalloonTooltip.Show(string.Join(" + ", this.pressedKeys.Select(k => k.ToString()).ToArray()));
            }
        }
    }
}
