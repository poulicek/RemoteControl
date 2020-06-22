using System.Collections.Generic;
using System.IO;
using System.Net;

namespace RemoteControl.Server
{
    public class HttpResponse
    {
        private readonly Stream stream;
        private readonly StreamWriter writer;
        private bool headerWritten;

        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();


        public HttpResponse(Stream stream)
        {
            this.stream = stream;
            this.writer = new StreamWriter(stream);
            this.Headers.Add("Cache-Control", "no-cache");
        }


        /// <summary>
        /// Writes a HTTP header
        /// </summary>
        private void writeHeader(string mime)
        {
            this.writer.WriteLine($"HTTP/1.0 {(int)this.StatusCode} {this.StatusCode}");

            this.Headers["Content-Type"] = mime;
            foreach (var h in this.Headers)
                this.writer.WriteLine($"{h.Key}: {h.Value}");
            this.writer.WriteLine();
            this.headerWritten = true;
        }


        /// <summary>
        /// Writes a string
        /// </summary>
        public void Write(string text)
        {
            if (!this.headerWritten)
                this.writeHeader("text/html");
            this.writer.Write(text);
        }


        /// <summary>
        /// Writes a byte array
        /// </summary>
        public void Write(byte[] bytes)
        {
            if (!this.headerWritten)
                this.writeHeader("application/octet-stream");
            this.writer.BaseStream.Write(bytes, 0, bytes.Length);
        }


        /// <summary>
        /// Writes a stream
        /// </summary>
        public void Write(Stream s)
        {
            if (!this.headerWritten)
                this.writeHeader("application/octet-stream");
            s.CopyTo(this.stream);
        }
    }
}
