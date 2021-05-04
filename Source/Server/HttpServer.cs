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
        public event Action ListeningChanged;
        public event Action<Exception> ErrorOccured;
        public event Action<HttpContext> RequestReceived;

        private bool disposing;
        private TcpListener listener;

        public int Port { get; }
        public bool IsListening { get; private set; }
        public string AllowOrigin { get; set; }       
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
            if (this.disposing)
                throw new InvalidOperationException("The HttpServer object is already disposed.");

            if (this.IsListening)
                return;            

            this.listener = new TcpListener(IPAddress.Any, this.Port);
            this.listener.Start();
            this.IsListening = true;

            this.keepListening();

            this.raiseListeningChanged();
        }


        /// <summary>
        /// Stops listing to incoming connetctions
        /// </summary>
        public void Stop()
        {
            this.IsListening = false;
            this.listener?.Stop();
        }


        /// <summary>
        /// Disposes the object
        /// </summary>
        public void Dispose()
        {
            this.disposing = true;
            this.Stop();
        }


        /// <summary>
        /// Starts the asynchronous listening
        /// </summary>
        private async void keepListening()
        {
            try
            {
                while (this.IsListening && !this.disposing)
                {
                    try
                    {
                        var tcpClient = await this.listener.AcceptTcpClientAsync();
                        this.handleTcpClientAsync(tcpClient);
                    }
                    catch (ObjectDisposedException) { break; }
                    catch (InvalidOperationException) { break; }
                    catch (Exception ex) { this.raiseError(ex); }
                }
            }
            finally { this.onConnectionBroke(); }
        }



        /// <summary>
        /// Handles the broken connection
        /// </summary>
        private void onConnectionBroke()
        {
            this.IsListening = false;
            if (!this.disposing)
                this.raiseListeningChanged();
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
                    while (this.IsListening)
                    {
                        using (var context = new HttpContext(stream, this.AllowOrigin))
                        {
                            this.processContext(context);
                            if (context.Response.CloseConnection)
                                break;
                        }
                    }
                }
            }
            catch (IOException) { }
            catch (AuthenticationException) { }
            catch (Exception ex) { this.raiseError(ex); }
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
        /// Raises an error event
        /// </summary>
        private void raiseError(Exception ex)
        {
            try
            {
                this.ErrorOccured?.Invoke(ex);
            }
            catch { }
        }


        /// <summary>
        /// Raises an error event
        /// </summary>
        private void raiseListeningChanged()
        {
            try
            {
                this.ListeningChanged?.Invoke();
            }
            catch { }
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
    }
}
