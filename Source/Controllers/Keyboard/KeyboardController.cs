using System.Windows.Forms;
using RemoteControl.Server;
using TrayToolkit.Helpers;

namespace RemoteControl.Controllers.Keyboard
{
    public class KeyboardController : IController
    {
        public void ProcessRequest(HttpContext context)
        {
            var realChar = context.Request.Query["h"];
            if (realChar?.Length > 0)
            {
                InputHelper.SendUnicodeChar(realChar[0]);
                return;
            }

            // reading the keyCode
            if (!int.TryParse(context.Request.Query["v"], out var keyCode))
                return;

            // skipping caps lock and modifier keys
            if (keyCode == (int)Keys.ProcessKey || keyCode == (int)Keys.CapsLock || keyCode == (int)Keys.ShiftKey)
                return;

            // executing the key pressing
            var scanMode = context.Request.Query["a"] == "1";
            var released = int.TryParse(context.Request.Query["s"], out var keyState) && keyState == 0;

            this.pressKey((Keys)keyCode, released, scanMode);
        }


        /// <summary>
        /// Key pressing actions
        /// </summary>
        private void pressKey(Keys key, bool release, bool scanMode)
        {
            if (release)
                key.Up(scanMode);
            else
                key.Down(scanMode);
        }
    }
}
