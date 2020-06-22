using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace RemoteControl.Server
{
    public class HttpResponse
    {
        private readonly Stream stream;

        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public byte[] Bytes { get; set; }
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();
        public string Content { set { this.Bytes = Encoding.UTF8.GetBytes(value); } }


        public HttpResponse(Stream stream)
        {
            this.stream = stream;
            this.Headers.Add("Content-Type", "text/html");
            this.Headers.Add("Cache-Control", "no-cache");
        }


        /// <summary>
        /// Writes the response to the stream
        /// </summary>
        public void Write()
        {
            if (this.Bytes == null)
                this.Bytes = new byte[0];

            // default to text/html content type
            this.Headers["Content-Length"] = this.Bytes.Length.ToString();

            this.write( string.Format("HTTP/1.0 {0} {1}\r\n", (int)this.StatusCode, this.StatusCode.ToString()));
            foreach (var h in this.Headers)
                this.write($"{h.Key}: {h.Value}\r\n");
            this.write("\r\n");
            this.write(this.Bytes);
        }


        private void write(string text)
        {
            this.write(Encoding.UTF8.GetBytes(text));
        }

        private void write(byte[] bytes)
        {
            this.stream.Write(bytes, 0, bytes.Length);
        }
    }
}
