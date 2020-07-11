using System;
using System.Windows.Forms;
using RemoteControl.Server;
using TrayToolkit.Helpers;

namespace RemoteControl.Controllers
{
    public class MediaController : IController, IDisposable
    {
        private readonly TrayToolkit.OS.Display.DisplayController display = new TrayToolkit.OS.Display.DisplayController();

        public void ProcessRequest(HttpContext context)
        {
            switch (context.Request.Query["v"])
            {
                case "brightnessUp":
                    this.display.SetBrightness(this.display.CurrentValue + 10);
                    break;

                case "brightnessDown":
                    this.display.SetBrightness(this.display.CurrentValue - 10);
                    break;

                case "screenOff":
                    this.display.TurnOff();
                    break;

                default:
                    if (int.TryParse(context.Request.Query["v"], out var keyCode))
                        ((Keys)keyCode).Down();
                    break;
            }
        }


        public void Dispose()
        {
            this.display.Dispose();
        }
    }
}
