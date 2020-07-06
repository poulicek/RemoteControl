using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace RemoteControl.Server
{
    public class HttpServer : IDisposable
    {
        private bool enabled;
        private TcpListener listener;

        public string AllowOrigin { get; set; }
        public event Action<Exception> ErrorOccured;
        public event Action<HttpContext> RequestReceived;

        public int Port { get; }

        public X509Certificate Certificate { get; }


        public string HostName
        {
            get
            {
                foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                        return ip.ToString();

                return "localhost";
            }
        }


        public HttpServer(int port, bool useHttps)
            : this(port, null)
        {
            if (useHttps)
                this.Certificate = this.makeCert();
        }


        public HttpServer(int port, X509Certificate cert)
        {
            this.Port = port;
            this.Certificate = cert;
        }


        /// <summary>
        /// Returns the current server url
        /// </summary>
        public string GetUrl(string hostName = null)
        {
            return $"{(this.Certificate == null ? "http" : "https")}://{hostName ?? this.HostName}:{this.Port}";
        }


        /// <summary>
        /// Starts listening to incoming connections
        /// </summary>
        public void Listen()
        {
            this.startListening(this.Port);
        }


        /// <summary>
        /// Starts the asynchornous listening
        /// </summary>
        private async void startListening(int port)
        {
            this.enabled = true;
            this.listener = new TcpListener(IPAddress.Any, port);
            this.listener.Start();

            while (this.enabled)
            {
                try
                {
                    var tcpClient = await this.listener.AcceptTcpClientAsync();
                    this.handleTcpClientAsync(tcpClient);
                }
                catch (ObjectDisposedException) { }
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
                using (var stream = this.getStream(o as TcpClient))
                {
                    while (this.enabled)
                        using (var context = new HttpContext(stream, this.AllowOrigin))
                            this.processContext(context);
                }
            }
            catch (AuthenticationException) { }
            catch (IOException) { }
            catch (Exception ex) { this.ErrorOccured?.Invoke(ex); }
        }



        /// <summary>
        /// Processes the context
        /// </summary>
        private void processContext(HttpContext context)
        {
            try
            {
                this.RequestReceived?.Invoke(context);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = HttpStatusCode.InternalServerError;
                context.Response.Write(ex.ToString());
            }
        }


        /// <summary>
        /// Gets a stream from the TCP client
        /// </summary>
        private Stream getStream(TcpClient tcpClient)
        {
            var tcpStream = tcpClient.GetStream();
            if (this.Certificate == null)
                return tcpStream;

            var sslStream = new SslStream(tcpStream, false);
            sslStream.AuthenticateAsServer(this.Certificate, false, SslProtocols.Tls12, false);
            return sslStream;
        }


        /// <summary>
        /// Creates a self-signed certificate
        /// </summary>
        private X509Certificate2 makeCert()
        {
            var req = new CertificateRequest("cn=" + this.HostName, ECDsa.Create(ECCurve.NamedCurves.nistP384), HashAlgorithmName.SHA256);
            var cert = req.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(5));
            return new X509Certificate2(cert.Export(X509ContentType.Pfx));
        }


        public void Dispose()
        {
            this.enabled = false;
            this.listener?.Stop();
        }
    }
}
