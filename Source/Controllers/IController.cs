using RemoteControl.Server;

namespace RemoteControl.Controllers
{
    interface IController
    {
        void ProcessRequest(HttpContext context);
    }
}
