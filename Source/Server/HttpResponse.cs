using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace RemoteControl.Server
{
    public class HttpResponse
    {
        private readonly Stream stream;
        private bool headerWritten;

        public TimeSpan? CacheAge { get; set; }
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();


        public HttpResponse(Stream stream, string allowOrigin)
        {
            this.stream = stream;
            this.Headers["Access-Control-Allow-Origin"] = allowOrigin;
        }


        /// <summary>
        /// Writes the response to the stream
        /// </summary>
        private void writeHeader(string mime, int length)
        {
            this.Headers["Content-Type"] = mime;
            this.Headers["Content-Length"] = length.ToString();

            if (this.CacheAge.HasValue && !this.Headers.ContainsKey("Cache-Control"))
                this.Headers["Cache-Control"] = this.CacheAge == TimeSpan.Zero ? "no-cache, no-store, must-revalidate" : "public, max-age=" + (long)this.CacheAge.Value.TotalSeconds;

            var sb = new StringBuilder();
            sb.AppendLine($"HTTP/1.1 {(int)this.StatusCode} {this.StatusCode}");

            foreach (var h in this.Headers)
                sb.AppendLine($"{h.Key}: {h.Value}");
            sb.AppendLine();

            this.headerWritten = true;
            this.Write(sb.ToString());            
        }


        public void Write()
        {
            if (!this.headerWritten)
                this.writeHeader(null, 0);
        }


        public void Write(string str, string mime = "text/html")
        {
            var bytes = Encoding.UTF8.GetBytes(str ?? string.Empty);
            if (!this.headerWritten)
                this.writeHeader(mime, bytes.Length);

            this.stream.Write(bytes, 0, bytes.Length);
        }


        public void Write(Stream s, string mime)
        {
            if (!this.headerWritten)
                this.writeHeader(mime, (int)(s?.Length ?? 0));

            s?.CopyTo(this.stream);
        }
    }
}