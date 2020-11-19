using System;
using System.Collections.Generic;
using System.Threading;
using RemoteControl.Controllers;
using RemoteControl.Server;
using TrayToolkit.Helpers;

namespace RemoteControl.Logic
{
    public class InputListener : IDisposable
    {
        private const int SERVER_PORT = 7211;
        private const int RETRY_CONN_AFTER_MS = 5000;

        private readonly HttpServer server = new HttpServer(SERVER_PORT, false) { AllowOrigin = "*" };
        private readonly Dictionary<string, IController> controllers = new Dictionary<string, IController>();

        private bool disposed;
        private Exception lastException;

        public bool IsConnected => this.server.IsListening;
        public string ServerUrl => this.server.GetUrl();
        public string AppVersion { get; } = ResourceHelper.GetLastWriteTime().GetHashCode().ToString("x");

        public event Action<bool> ConnectedChanged;
        public event Action<Exception> ConnectionError;


        public InputListener()
        {
            this.server.ErrorOccured += this.onHttpErrorOccured;
            this.server.RequestReceived += this.ProcessRequest;
            this.server.ListeningChanged += this.onListeningChanged;

            this.initControllers();
        }

        #region Interface

        /// <summary>
        /// Starts the server receiving the requests
        /// </summary>
        public void StartServer()
        {
            try
            {
                // naive aproach to speed up the startup
                this.server.Listen();
            }
            catch
            {
                // robust aproach when the naive fails
                this.lastException = null;
                this.keepStarting();
            }
        }


        /// <summary>
        /// Handles the request reception
        /// </summary>
        public void ProcessRequest(HttpContext context)
        {
            var command = context.Request.Query["c"] ?? "file";

            if (command == "suspend")
                this.suspendServer(context);
            else if (!this.controllers.TryGetValue(command, out var ctrl))
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

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles listeining stopped event
        /// </summary>
        private void onListeningChanged()
        {
            this.ConnectedChanged?.Invoke(this.IsConnected);
        }


        /// <summary>
        /// Handles the request error
        /// </summary>
        private void onHttpErrorOccured(Exception ex)
        {
            ThreadingHelper.HandleException(ex);
        }

        #endregion


        /// <summary>
        /// Initializes the controllers
        /// </summary>
        private void initControllers()
        {
            var serverUrl = this.ServerUrl;

            this.controllers.Add("file", new FilesController(this.AppVersion, this.server.GetUrl(Environment.MachineName)));
            this.controllers.Add("app", new AppController(this.AppVersion, serverUrl));
            this.controllers.Add("view", new ViewController(serverUrl));
            this.controllers.Add("key", new KeysController());
            this.controllers.Add("media", new MediaController());
            this.controllers.Add("grip", new GripController());
            this.controllers.Add("menu", new MenuController());
        }


        /// <summary>
        /// Starts the server receiving the requests
        /// </summary>
        private void keepStarting()
        {
            try
            {
                if (this.IsConnected || this.disposed)
                    return;

                this.suspendOtherInstance();
                this.server.Listen();
            }
            catch (Exception ex)
            {
                this.retryServerStart();
                this.raiseFirstTimeError(ex);
            }
        }


        /// <summary>
        /// Restarts the server after the given period of time
        /// </summary>
        private void retryServerStart(int delayMs = RETRY_CONN_AFTER_MS)
        {
            new Timer(o => this.keepStarting(), null, delayMs, Timeout.Infinite);
        }


        /// <summary>
        /// Suspends the current server instance
        /// </summary>
        private void suspendServer(HttpContext context)
        {
            context.Response.Write();
            this.server.Stop();
            this.lastException = new Exception("App suspended by another running instance");
            this.ConnectionError?.Invoke(this.lastException);
        }

        
        /// <summary>
        /// Tries to suspend the existing server
        /// </summary>
        private void suspendOtherInstance()
        {
            using (var wc = new TimedWebClient(1000))
                wc.UrlExists(this.ServerUrl + "?c=suspend");
            Thread.Sleep(100);
        }


        /// <summary>
        /// Registers the last error and rises an event if it's the first one
        /// </summary>
        private void raiseFirstTimeError(Exception ex)
        {
            try
            {
                if (this.lastException == null)
                    this.ConnectionError?.Invoke(ex);
                this.lastException = ex;
            }
            catch { }
        }
    }
}
