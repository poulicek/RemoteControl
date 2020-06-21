using System;
using System.IO;
using System.Net;
using System.Threading;

namespace RemoteControl.Server
{
    public class HttpServerOd : IDisposable
    {
        private readonly HttpListener listener = new HttpListener();

        public event Action<string, string> CommandReceived;

        private HttpListenerContext context;


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
                    var result = this.listener.BeginGetContext(this.handleRequest, this.listener);
                    result.AsyncWaitHandle.WaitOne();
                }
                catch { }
            }
        }


        private void handleRequest(IAsyncResult result)
        {
            try
            {
                this.context = this.listener.EndGetContext(result);
                this.CommandReceived?.Invoke(context.Request.QueryString["command"], context.Request.QueryString["value"]);

            }
            finally
            {
                this.context.Response.Close();
                this.context = null;
            }
        }


        public void WriteText(string text, HttpStatusCode httpStatus = HttpStatusCode.OK)
        {
            if (this.context == null)
                return;

            this.context.Response.ContentType = "text/html";
            using (var w = new StreamWriter(this.context.Response.OutputStream))
                w.Write(text);
            this.context.Response.OutputStream.Close();
        }

        public void Dispose()
        {
            this.listener.Stop();
            this.listener.Close();
        }
    }
}
