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

        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();


        public HttpResponse(Stream stream)
        {
            this.stream = stream;
        }


        /// <summary>
        /// Writes the response to the stream
        /// </summary>
        private void writeHeader(string mime, int length)
        {
            this.Headers["Content-Type"] = mime;
            this.Headers["Content-Length"] = length.ToString();
#if DEBUG
            this.Headers["Cache-Control"] = "no-cache";
#endif

            this.writeLine($"HTTP/1.1 {(int)this.StatusCode} {this.StatusCode}");
            foreach (var h in this.Headers)
                this.writeLine($"{h.Key}: {h.Value}");
            this.writeLine();
            this.headerWritten = true;
        }


        private void writeLine(string str = null)
        {
            var bytes = Encoding.UTF8.GetBytes(str + "\r\n");
            this.stream.Write(bytes, 0, bytes.Length);
        }


        public void WriteEmpty()
        {
            if (!this.headerWritten)
                this.writeHeader(null, 0);
        }


        public void Write(Stream s, string mime)
        {
            if (!this.headerWritten)
                this.writeHeader(mime, (int)(s?.Length ?? 0));

            s?.CopyTo(this.stream);
        }
    }
}