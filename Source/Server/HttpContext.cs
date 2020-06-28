using System;
using System.IO;

namespace RemoteControl.Server
{
    public class HttpContext : IDisposable
    {
        public HttpRequest Request { get; }
        public HttpResponse Response { get; }


        public HttpContext(Stream stream, string allowOrigin = null)
        {
            this.Request = new HttpRequest(stream);
            this.Response = new HttpResponse(stream, allowOrigin);
        }


        public void Dispose()
        {
            this.Response.Write();
        }
    }
}

