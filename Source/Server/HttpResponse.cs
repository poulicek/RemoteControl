using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace RemoteControl.Server
{
    public class HttpResponse
    {
        private readonly Stream stream;
        private bool headerWritten;

        public bool Infinite { get; private set; }
        public Stream Stream { get { return this.stream; } }
        public TimeSpan? CacheAge { get; set; }
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();


        public HttpResponse(Stream stream)
        {
            this.stream = stream;
        }


        /// <summary>
        /// Writes the response to the stream
        /// </summary>
        public void WriteHeader(string mime, int length)
        {
            this.Infinite = length == 0;

            this.Headers["Content-Type"] = mime;

            if (length > 0)
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


        /// <summary>
        /// Returns the MIME type
        /// </summary>
        public string GetMime(string fileName)
        {
            return MimeMapping.GetMimeMapping(fileName);
        }


        public void Write()
        {
            if (!this.headerWritten)
                this.WriteHeader(null, 0);
        }


        public void Write(string str, string mime = "text/html")
        {
            this.Write(Encoding.UTF8.GetBytes(str ?? string.Empty), mime);
        }


        public void Write(byte[] bytes, string mime = "text/html")
        {
            if (!this.headerWritten)
                this.WriteHeader(mime, bytes?.Length ?? 0);

            if (bytes?.Length > 0)
                this.stream.Write(bytes, 0, bytes.Length);
        }


        public void Write(Stream s, string mime = "text/html")
        {
            if (!this.headerWritten)
                this.WriteHeader(mime, (int)(s?.Length ?? 0));

            s.Seek(0, SeekOrigin.Begin);
            s.CopyTo(this.stream);
        }
    }
}