using System.Net;
using RemoteControl.Server;
using TrayToolkit.Helpers;

namespace RemoteControl.Controllers
{
    public class ViewController : IController
    {
        public void ProcessRequest(HttpContext context)
        {
            var view = context.Request.Query["v"];
            var res = ResourceHelper.GetResourceStream($"App.Views.{view}.html");

            if (res != null)
                context.Response.Write(res);
            else
            {
                context.Response.StatusCode = HttpStatusCode.NotFound;
                context.Response.Write($"The view '{view}' isn't available");
            }            
        }
    }
}