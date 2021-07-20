using System;
using System.Windows.Forms;
using RemoteControl.Server;
using TrayToolkit.Helpers;

namespace RemoteControl.Controllers.Keyboard
{
    public class KeyboardController : IController
    {
        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Query["p"] == "1")
            {
                if (!this.tryProcessKeyCode(context))
                    this.tryProcessChar(context);
            }
            else
            {
                if (!this.tryProcessChar(context))
                    this.tryProcessKeyCode(context);
            }
        }


        /// <summary>
        /// Processes the character if provided
        /// </summary>
        private bool tryProcessChar(HttpContext context)
        {
            var realChar = context.Request.Query["h"];
            if (string.IsNullOrEmpty(realChar))
                return false;

            InputHelper.SendUnicodeChar(realChar[0]);
            return true;
        }


        /// <summary>
        /// Processes the key code if provided
        /// </summary>
        private bool tryProcessKeyCode(HttpContext context)
        {
            // reading the keyCode
            if (!int.TryParse(context.Request.Query["v"], out var keyCode))
                return false;

            // skipping caps lock and modifier keys
            if (keyCode == (int)Keys.ProcessKey || keyCode == (int)Keys.CapsLock || keyCode == (int)Keys.ShiftKey)
                return false;

            // executing the key pressing
            var scanMode = context.Request.Query["a"] == "1";

            if (int.TryParse(context.Request.Query["s"], out var keyState))
                this.pressKey((Keys)keyCode, scanMode, keyState == 0);
            else
                this.pressKey((Keys)keyCode, scanMode);

            return true;
        }


        /// <summary>
        /// Pressess and releases the key
        /// </summary>
        private void pressKey(Keys key, bool scanMode = false)
        {
            this.pressKey(key, scanMode, false);
            this.pressKey(key, scanMode, true);
        }


        /// <summary>
        /// Key pressing actions
        /// </summary>
        private void pressKey(Keys key, bool scanMode, bool release)
        {
            if (release)
                key.Up(scanMode);
            else
                key.Down(scanMode);
        }
    }
}
