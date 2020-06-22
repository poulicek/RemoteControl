using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Windows.Forms;
using RemoteControl.Server;
using TrayToolkit.Helpers;
using TrayToolkit.IO.Display;
using TrayToolkit.UI;

namespace RemoteControl
{
    public class Controller : IDisposable
    {
        private const int Port = 5000;
        private readonly HttpServer server = new HttpServer();
        private readonly DisplayContoller display = new DisplayContoller();


        public string ServerUrl
        {
            get
            {
                foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                        return $"http://{ip}:{Port}";
                
                throw new Exception("The computer is not connected to the network");
            }
        }


        public Controller()
        {
            this.server.ErrorOccured += onHttpErrorOccured;
            this.server.RequestReceived += this.onRequestReceived;
            this.server.Listen(Controller.Port);
        }

        private void onHttpErrorOccured(Exception ex)
        {
#if DEBUG
            BalloonTooltip.Show(ex.Message, null, ex.StackTrace);
#endif
        }

        private void onRequestReceived(HttpContext context)
        {
            var command = context.Request.Query["command"];
            var value = context.Request.Query["value"];
            switch (command)
            {
                case "key":
                    this.processKeyCommand(value);
                    break;

                case "display":
                    this.processDisplayCommand(value);
                    break;

                default:
                    var file = context.Request.Url.Trim('/').Replace('/', '.');
                    context.Response.Write(this.getResource(string.IsNullOrEmpty(file) ? "index.html" : file));
                    break;
            }
        }


        private void processKeyCommand(string value)
        {
            if (int.TryParse(value, out var keyCode))
                ((Keys)keyCode).Press();
        }

        private void processDisplayCommand(string value)
        {
            switch (value)
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


        private Stream getResource(string fileName)
        {
            var localFile = Path.Combine("../../Resources", fileName);
            if (File.Exists(localFile))
                return new FileStream(localFile, FileMode.Open);

            return Assembly.GetExecutingAssembly().GetManifestResourceStream("RemoteControl.App." + fileName);
        }


        public void Dispose()
        {
            this.server.Dispose();
            this.display.Dispose();
        }
    }
}
