using RemoteControl.Server;

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
            switch (context.Request.Query["value"])
            {
                case "getversion":
                    context.Response.Write(this.appVersion);
                    break;
            }
        }
    }
}
