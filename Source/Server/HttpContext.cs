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

        public HttpContext(Stream stream)
        {
            this.stream = stream;
            this.Request = new HttpRequest(stream);
            this.Response = new HttpResponse(stream);
        }


        public void Dispose()
        {
            this.stream.Flush();
            this.stream.Dispose();
        }
    }
}

