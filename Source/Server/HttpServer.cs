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
                this.listener.BeginAcceptTcpClient(this.onTcpClientAccepted, this.listener);
        }

        private void onTcpClientAccepted(IAsyncResult ar)
        {
            var listener = (TcpListener)ar.AsyncState;
            var tcpClient = listener.EndAcceptTcpClient(ar);

            using (var context = new HttpContext(tcpClient))
                this.RequestReceived?.Invoke(context);
        }


        public void Dispose()
        {
            this.enabled = false;
            this.listener?.Stop();
        }
    }
}
