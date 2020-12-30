using System;
using System.Threading;
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

                case "standby":
                    this.setSuppendStateAsync();
                    break;

                default:
                    if (int.TryParse(context.Request.Query["v"], out var keyCode))
                        ((Keys)keyCode).Down();
                    break;
            }
        }


        private void setSuppendStateAsync()
        {
            ThreadingHelper.DoAsync(() =>
            {
                var time = DateTime.Now;

                // turning of the display triggers sleep if S0 power state is supported
                this.display.TurnOff();

                // giving a chance the computer to go to sleep
                Thread.Sleep(3000);

                // turning on standby if the computer remained active while the screen was off
                if ((DateTime.Now - time).TotalSeconds < 3.5)
                    Application.SetSuspendState(PowerState.Suspend, true, true);
            });
        }


        public void Dispose()
        {
            this.display.Dispose();
        }
    }
}
