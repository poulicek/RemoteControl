using System;
using RemoteControl.Server;
using TrayToolkit.Helpers;

namespace RemoteControl.Controllers
{
    public class AppController : IController
    {
        private readonly string appVersion;

        public AppController(string appVersion)
        {
            this.appVersion = appVersion;
        }

        public void ProcessRequest(HttpContext context)
        {
            switch (context.Request.Query["v"])
            {
                case "getversion":
                    context.Response.Write($"{this.appVersion},{Environment.MachineName}");
                    break;
            }
        }
    }
}
