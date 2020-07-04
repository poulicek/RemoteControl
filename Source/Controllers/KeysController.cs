using System.Windows.Forms;
using RemoteControl.Server;
using TrayToolkit.Helpers;

namespace RemoteControl.Controllers
{
    public class KeysController : IController
    {
        public void ProcessRequest(HttpContext context)
        {
            var value = context.Request.Query["value"];
            if (!string.IsNullOrEmpty(value) && int.TryParse(value, out var keyCode))
                ((Keys)keyCode).Press();
        }
    }
}
