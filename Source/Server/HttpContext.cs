using System;
using System.IO;

namespace RemoteControl.Server
{
    public class HttpContext : IDisposable
    {
        public HttpRequest Request { get; }
        public HttpResponse Response { get; }


        private HttpContext(Stream stream)
        {
            this.Request = new HttpRequest(stream);
            this.Response = new HttpResponse(stream, this.Request.IsGZipAccepted);
        }


        public static HttpContext Read(Stream stream)
        {
            return new HttpContext(stream);
        }

        public void Dispose()
        {
            this.Response.Write();
        }
    }
}

