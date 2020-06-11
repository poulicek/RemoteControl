using System;
using System.IO;
using System.Net;
using System.Threading;

namespace RemoteControl.Server
{
    public class HttpServer : IDisposable
    {
        private readonly HttpListener listener = new HttpListener();

        public event Action<string, string> CommandReceived;

        private HttpListenerContext currentContext;

        public void Listen(int port)
        {
            ThreadPool.QueueUserWorkItem((o) => this.startListening(port));
        }


        private void startListening(int port)
        {
            this.listener.Prefixes.Add("http://*:" + port + "/");
            this.listener.Start();

            while (this.listener.IsListening)
            {
                try
                {
                    this.handleRequest(this.listener.GetContext());
                }
                catch { }
            }
        }


        private void handleRequest(HttpListenerContext context)
        {
            this.currentContext = context;
            this.CommandReceived?.Invoke(context.Request.QueryString["command"], context.Request.QueryString["value"]);
        }


        public void WriteText(string text, HttpStatusCode httpStatus = HttpStatusCode.OK)
        {
            if (this.currentContext == null)
                return;

            this.currentContext.Response.ContentType = "text/html";
            using (var w = new StreamWriter(this.currentContext.Response.OutputStream))
                w.Write(text);
            this.currentContext.Response.OutputStream.Close();
        }

        public void Dispose()
        {
            this.listener.Stop();
            this.listener.Close();
        }
    }
}
