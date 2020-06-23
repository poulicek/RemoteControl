using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

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
            this.startListening(port);
        }


        private async void startListening(int port)
        {
            this.enabled = true;
            this.listener = new TcpListener(IPAddress.Any, port);
            this.listener.Start();

            while (this.enabled)
            {
                var tcpClient = await this.listener.AcceptTcpClientAsync();
                this.handleTcpClientAsync(tcpClient);
            }
        }


        /// <summary>
        /// Handles the new client connection
        /// </summary>
        private void handleTcpClientAsync(TcpClient tcpClient)
        {
            Task.Factory.StartNew(this.readWriteData, tcpClient, TaskCreationOptions.LongRunning);
        }


        /// <summary>
        /// Reads and writes data to the network stream
        /// </summary>
        private void readWriteData(object o)
        {
            try
            {
                // The stream gets reused indefinitely so the connection is kept alive
                using (var s = (o as TcpClient).GetStream())
                    while (this.enabled)
                        using (var context = new HttpContext(s))
                            this.RequestReceived?.Invoke(context);
            }
            catch (Exception ex) { this.ErrorOccured?.Invoke(ex); }
        }


        public void Dispose()
        {
            this.enabled = false;
            this.listener?.Stop();
        }
    }
}
