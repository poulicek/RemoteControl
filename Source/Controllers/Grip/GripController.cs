using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using RemoteControl.Server;

namespace RemoteControl.Controllers.Grip
{
    public class GripController : IController
    {
        private readonly Dictionary<string, IGripButton> buttons = new Dictionary<string, IGripButton>();


        public virtual void ProcessRequest(HttpContext context)
        {
            var keyCodes = context.Request.Query["v"];
            var button = this.getButton(keyCodes);

            if (button == null)
                return;

            // setting the scan mode
            if (button is GripButtonKeys buttonKeys)
                buttonKeys.ScanMode = context.Request.Query["a"] == "1";

            // pressing or releasing the button
            if (int.TryParse(context.Request.Query["s"], out var keyState) && keyState == 0)
                button.Release();
            else
                button.Press(this.parseCoords(context.Request.Query["o"]?.Split(',')));                
        }


        /// <summary>
        /// Returns the relevant grip button instance
        /// </summary>
        private IGripButton getButton(string keyCodes)
        {
            if (string.IsNullOrEmpty(keyCodes))
                return null;

            lock (this.buttons)
            {
                if (!this.buttons.TryGetValue(keyCodes, out var button))
                    this.buttons.Add(keyCodes, button = new GripButtonKeys(this.parseKeys(keyCodes.Split(','))));

                return button;
            }
        }

        #region Parsing

        /// <summary>
        /// Parses the pressed keys
        /// </summary>
        protected PointF parseCoords(string[] coords)
        {
            if (coords?.Length != 2 ||
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
