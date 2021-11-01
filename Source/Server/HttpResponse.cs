using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Web;

namespace RemoteControl.Server
{
    public class HttpResponse
    {
        private readonly Stream stream;
       
        private bool headerWritten;

        public bool ApplyGzipCompression { get; set; }
        public bool Infinite { get; private set; }
        public Stream Stream { get { return this.stream; } }
        public TimeSpan? CacheAge { get; set; }
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();


        public HttpResponse(Stream stream, bool useGZipEncoding)
        {
            this.stream = stream;
            this.ApplyGzipCompression = useGZipEncoding;
        }


        /// <summary>
        /// Writes the response to the stream
        /// </summary>
        public void WriteHeader(string mime, int? length = null)
        {
            this.Infinite = !length.HasValue;

            if (this.ApplyGzipCompression)
                this.Headers["Content-Encoding"] = "gzip";

            if (!string.IsNullOrEmpty(mime))
                this.Headers["Content-Type"] = mime;

            if (length.HasValue)
                this.Headers["Content-Length"] = length.ToString();

            if (this.CacheAge.HasValue && !this.Headers.ContainsKey("Cache-Control"))
                this.Headers["Cache-Control"] = this.CacheAge == TimeSpan.Zero ? "no-cache, no-store, must-revalidate" : "public, max-age=" + (long)this.CacheAge.Value.TotalSeconds;

            var sb = new StringBuilder();
            sb.AppendLine($"HTTP/1.1 {(int)this.StatusCode} {this.StatusCode}");

            foreach (var h in this.Headers)
                sb.AppendLine($"{h.Key}: {h.Value}");
            sb.AppendLine();

            this.headerWritten = true;

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            this.stream.Write(bytes, 0, bytes.Length);
        }


        /// <summary>
        /// Returns the MIME type
        /// </summary>
        public string GetMime(string fileName)
        {
            return MimeMapping.GetMimeMapping(fileName);
        }


        /// <summary>
        /// Compreses the stream into GZip
        /// </summary>
        private Stream compressStream(Stream input)
        {
            var result = new MemoryStream();
            using (var cs = new GZipStream(result, CompressionMode.Compress, true))
            {
                input.Seek(0, SeekOrigin.Begin);
                input.CopyTo(cs);
            }

            return result;
        }

        #region Stream Writing

        public void Write()
        {
            this.writeStream(null, null);
        }


        public void Write(string str, string mime = "text/html")
        {
            this.Write(Encoding.UTF8.GetBytes(str ?? string.Empty), mime);
        }


        public void Write(byte[] bytes, string mime = "text/html")
        {
            using (var ms = new MemoryStream(bytes))
                this.Write(ms, mime);
        }


        public void Write(Stream s, string mime = "text/html")
        {
            if (!this.ApplyGzipCompression)
                this.writeStream(s, mime);
            else
            {
                using (var cs = this.compressStream(s))
                    this.writeStream(cs, mime);
            }
        }


        private void writeStream(Stream s, string mime)
        {
            if (!this.headerWritten)
                this.WriteHeader(mime, (int)(s?.Length ?? 0));

            if (s != null)
            {
                s.Seek(0, SeekOrigin.Begin);
                s.CopyTo(this.stream);
            }
        }

        #endregion
    }
}