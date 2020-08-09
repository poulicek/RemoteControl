using System;
using System.Collections.Generic;
using System.Threading;
using RemoteControl.Server;
using TrayToolkit.Helpers;

namespace RemoteControl.Controllers
{
    public class MainController : IDisposable
    {
        private bool disposed;
        private readonly HttpServer server = new HttpServer(7211, false) { AllowOrigin = "*" };
        private readonly Dictionary<string, IController> controllers = new Dictionary<string, IController>();

        public bool IsConnected { get { return this.server.IsListening; } }
        public string ServerUrl { get { return this.server.GetUrl(); } }

        public string AppVersion { get; } = ResourceHelper.GetLastWriteTime().GetHashCode().ToString("x");

        public Exception LastException { get; private set; }

        public event Action<bool> ConnectedChanged;
        public event Action<Exception> ConnectionError;

        public MainController()
        {
            this.server.ErrorOccured += this.onHttpErrorOccured;
            this.server.RequestReceived += this.ProcessRequest;

            this.controllers.Add("file", new FilesController(this.AppVersion, this.server.GetUrl(Environment.MachineName)));
            this.controllers.Add("app", new AppController(this.AppVersion, this.ServerUrl));
            this.controllers.Add("view", new ViewController());
            this.controllers.Add("key", new KeysController());
            this.controllers.Add("media", new MediaController());
            this.controllers.Add("grip", new GripController());
        }


        /// <summary>
        /// Starts the server receiving the requests
        /// </summary>
        public void StartServer()
        {
            if (this.disposed)
                return;

            try
            {
                this.server.Listen();
                this.LastException = null;
                this.ConnectedChanged?.Invoke(this.IsConnected);
            }
            catch (Exception ex) { this.onServerListeningError(ex, 5000); }
        }


        /// <summary>
        /// Handles server listening error
        /// </summary>
        private void onServerListeningError(Exception ex, int startAgainAfterMs = 0)
        {
            try
            {
                // propagating the first-time error
                if (this.LastException == null)
                    this.ConnectionError?.Invoke(ex);

                this.LastException = ex;
                this.ConnectedChanged?.Invoke(this.IsConnected);

                if (startAgainAfterMs > 0)
                    new Timer(o => StartServer(), null, startAgainAfterMs, Timeout.Infinite);
            }
            catch (Exception ex2) { ThreadingHelper.HandleException(ex2); }
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
            this.disposed = true;
            this.server.Dispose();

            foreach (var ctrl in this.controllers.Values)
                (ctrl as IDisposable)?.Dispose();
        }
    }
}
