using System;
using RemoteControl.Server;

namespace RemoteControl.Controllers
{
    public class DisplayController : IController, IDisposable
    {
        private readonly TrayToolkit.IO.Display.DisplayController display = new TrayToolkit.IO.Display.DisplayController();

        public void ProcessRequest(HttpContext context)
        {
            switch (context.Request.Query["value"])
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
            }
        }


        public void Dispose()
        {
            this.display.Dispose();
        }
    }
}
