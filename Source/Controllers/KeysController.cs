using System.Windows.Forms;
using RemoteControl.Server;
using TrayToolkit.Helpers;
using TrayToolkit.OS.Input;
using TrayToolkit.UI;

namespace RemoteControl.Controllers
{
    public class KeysController : IController
    {
        public void ProcessRequest(HttpContext context)
        {
            if (!int.TryParse(context.Request.Query["v"], out var keyCode))
                return;

#if DEBUG
            BalloonTooltip.Show(((ActionKey)(Keys)keyCode).ToString());
#endif

            var scanMode = context.Request.Query["a"] == "1";
            if (!int.TryParse(context.Request.Query["s"], out var keyState) || keyState == 1)
                ((Keys)keyCode).Down(scanMode);
            else
                ((Keys)keyCode).Up(scanMode);
        }
    }
}
