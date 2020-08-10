using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using RemoteControl.Server;
using TrayToolkit.Helpers;
using TrayToolkit.UI;

namespace RemoteControl.Controllers
{
    public class GripController : IController
    {
        #region Grip Button

        private class GripButton
        {
            private readonly Keys[] keys;
            private readonly List<Keys> pressedKeys = new List<Keys>();
            

            public GripButton(Keys[] keys)
            {
                this.keys = keys;
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
            /// Releases the button
            /// </summary>
            public void Release()
            {
                this.Press(new Keys[0]);
            }


            /// <summary>
            /// Presses the keys according to the coordinates
            /// </summary>
            public void Press(PointF coords)
            {
                this.Press(this.getPressedKeys(this.keys, coords));
            }


            /// <summary>
            /// Presses the keys
            /// </summary>
            public void Press(Keys[] keys)
            {
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

#if DEBUG
                    BalloonTooltip.Show(string.Join(" + ", this.pressedKeys.Select(k => k.ToString()).ToArray()));
#endif
                }
            }
        }

        #endregion

        private readonly Dictionary<string, GripButton> buttons = new Dictionary<string, GripButton>();


        public void ProcessRequest(HttpContext context)
        {
            var keyCodes = context.Request.Query["v"];
            var button = this.getButton(keyCodes);

            if (button == null)
                return;

            if (int.TryParse(context.Request.Query["s"], out var keyState) && keyState == 0)
                button.Release();
            else
                button.Press(this.parseCoords(context.Request.Query["o"].Split(',')));
                
        }


        /// <summary>
        /// Returns the relevant grip button instance
        /// </summary>
        private GripButton getButton(string keyCodes)
        {
            if (string.IsNullOrEmpty(keyCodes))
                return null;

            if (!this.buttons.TryGetValue(keyCodes, out var button))
                this.buttons[keyCodes] = button = new GripButton(this.parseKeys(keyCodes.Split(',')));

            return button;
        }

        #region Parsing

        /// <summary>
        /// Parses the pressed keys
        /// </summary>
        private PointF parseCoords(string[] coords)
        {
            if (coords.Length != 2 ||
                !float.TryParse(coords[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var x) ||
                !float.TryParse(coords[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var y))
                return PointF.Empty;

            return new PointF(x, y);
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

        #endregion
    }
}
