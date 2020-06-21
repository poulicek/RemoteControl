using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace RemoteControl.Server
{
    public class HttpServer : IDisposable
    {
        private bool enabled;
        private TcpListener listener;

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
            using (var context = new HttpContext(o as TcpClient))
                this.RequestReceived?.Invoke(context);
        }


        public void Dispose()
        {
            this.enabled = false;
            this.listener?.Stop();
        }
    }
}
