using System.Windows.Forms;
using RemoteControl.Server;
using TrayToolkit.Helpers;

namespace RemoteControl.Controllers
{
    public class KeysController : IController
    {
        private bool shiftKeyPressed;

        public void ProcessRequest(HttpContext context)
        {
            if (!int.TryParse(context.Request.Query["v"], out var keyCode))
                return;

            // executing the key pressing
            var scanMode = context.Request.Query["a"] == "1";

            // skipping caps lock as its release is not reported reliable by iOS
            if (keyCode == (int)Keys.CapsLock)
                return;

            if (int.TryParse(context.Request.Query["s"], out var keyState) && keyState == 0)
                ((Keys)keyCode).Up(scanMode);
            else
                ((Keys)keyCode).Down(scanMode);

            // releasing the shift key if previously pressed
            if (this.shiftKeyPressed)
                (Keys.ShiftKey).Up();

            this.shiftKeyPressed = keyCode == (int)Keys.ShiftKey;
            
        }
    }
}
