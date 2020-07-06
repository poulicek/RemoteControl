using System;
using System.Collections.Generic;
using RemoteControl.Server;
using TrayToolkit.Helpers;

namespace RemoteControl.Controllers
{
    public class MainController : IDisposable
    {
        private readonly HttpServer server = new HttpServer(5000, false) { AllowOrigin = "*" };
        private readonly Dictionary<string, IController> controllers = new Dictionary<string, IController>();

        public string ServerUrl { get { return this.server.GetUrl(); } }

        public string AppVersion { get; } = ResourceHelper.GetLastWriteTime().GetHashCode().ToString("x");


        public MainController()
        {
            this.server.ErrorOccured += this.onHttpErrorOccured;
            this.server.RequestReceived += this.ProcessRequest;
            this.server.Listen();

            this.controllers.Add("file", new FilesController(this.AppVersion, this.server.GetUrl(Environment.MachineName)));
            this.controllers.Add("app", new AppController(this.AppVersion));
            this.controllers.Add("key", new KeysController());
            this.controllers.Add("display", new DisplayController());
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
        public void ProcessRequest(HttpContext context)
        {
            var command = context.Request.Query["c"] ?? "file";
            if (!this.controllers.TryGetValue(command, out var ctrl))
                context.Response.StatusCode = System.Net.HttpStatusCode.BadRequest;
            else
            {
                context.Response.CacheAge = TimeSpan.Zero;
                ctrl.ProcessRequest(context);
            }   
        }


        public void Dispose()
        {
            this.server.Dispose();

            foreach (var ctrl in this.controllers.Values)
                (ctrl as IDisposable)?.Dispose();
        }
    }
}
