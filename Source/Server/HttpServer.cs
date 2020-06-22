using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace RemoteControl.Server
{
    public class HttpServer : IDisposable
    {
        private bool enabled;
        private TcpListener listener;

        public event Action<Exception> ErrorOccured;
        public event Action<HttpContext> RequestReceived;


        public void Listen(int port)
        {
            ThreadPool.QueueUserWorkItem((o) => this.startListening(port));
        }


        private void startListening(int port)
        {
            this.enabled = true;
            this.listener = new TcpListener(IPAddress.Any, port);
            this.listener.Start();

            while (this.enabled)
            {
                var tcpClient = this.listener.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(this.handleTcpClient, tcpClient);
            }
        }


        private void handleTcpClient(object o)
        {
            Stream stream = null;
            try
            {
                stream = (o as TcpClient).GetStream();
                using (var context = new HttpContext(stream))
                    this.RequestReceived?.Invoke(context);
            }
            catch (Exception ex) { this.ErrorOccured?.Invoke(ex); }
            finally { stream?.Close(); }
        }


        public void Dispose()
        {
            this.enabled = false;
            this.listener?.Stop();
        }
    }
}
