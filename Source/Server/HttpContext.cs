using System;
using System.IO;
using System.Net.Sockets;

namespace RemoteControl.Server
{
    public class HttpContext : IDisposable
    {
        private readonly Stream stream;

        public HttpRequest Request { get; }
        public HttpResponse Response { get; }

        public HttpContext(TcpClient tcpClient)
        {
            this.stream = tcpClient.GetStream();
            this.Request = new HttpRequest(stream);
            this.Response = new HttpResponse(stream);
        }


        public void Dispose()
        {
            this.Response.Write();
            this.stream.Flush();
            this.stream.Dispose();
        }
    }
}

