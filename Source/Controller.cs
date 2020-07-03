using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using RemoteControl.Server;
using TrayToolkit.Helpers;
using TrayToolkit.IO.Display;
using static RemoteControl.Server.HttpResponse;

namespace RemoteControl
{
    public class Controller : IDisposable
    {
        private readonly HttpServer server = new HttpServer(5000, false) { AllowOrigin = "*" };
        private readonly DisplayContoller display = new DisplayContoller();


        public string ServerUrl { get { return this.server.Url; } }


        public Controller()
        {
            this.server.ErrorOccured += this.onHttpErrorOccured;
            this.server.RequestReceived += this.onRequestReceived;
            this.server.Listen();
        }


        /// <summary>
        /// Handles the request error
        /// </summary>
        private void onHttpErrorOccured(Exception ex)
        {
            ThreadingHelper.HandleException(ex);
        }


        /// <summary>
        /// Handles the request reception
        /// </summary>
        private void onRequestReceived(HttpContext context)
        {
            if (context.Request.Query.Count == 0)
                this.writeResourceFile(context);
            else
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
                        context.Response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                        break;
                }
            }
        }


        /// <summary>
        /// Processes a key command
        /// </summary>
        private void processKeyCommand(string value)
        {
            if (int.TryParse(value, out var keyCode))
                ((Keys)keyCode).Press();
        }


        /// <summary>
        /// Processes a display command
        /// </summary>
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


        /// <summary>
        /// Writes a file from the resources
        /// </summary>
        private void writeResourceFile(HttpContext context)
        {
            var path = context.Request.Url.Trim('/').Replace('/', '.');
            var file = string.IsNullOrEmpty(path) ? "index.html" : path;
            var resource = this.getResource(file);

            if (resource == null)
                context.Response.StatusCode = System.Net.HttpStatusCode.NotFound;
            else
            {
#if !DEBUG
                context.Response.Cache = CacheControl.NoCache;
#endif
                context.Response.Write(resource, System.Web.MimeMapping.GetMimeMapping(file));
            }
        }


        /// <summary>
        /// Returns a stream representinga file in resources
        /// </summary>
        private Stream getResource(string fileName)
        {
            var localFile = Path.Combine("../../App", fileName);
            if (File.Exists(localFile))
                return new FileStream(localFile, FileMode.Open, FileAccess.Read);

            return Assembly.GetExecutingAssembly().GetManifestResourceStream("RemoteControl.App." + fileName);
        }


        public void Dispose()
        {
            this.server.Dispose();
            this.display.Dispose();
        }
    }
}
