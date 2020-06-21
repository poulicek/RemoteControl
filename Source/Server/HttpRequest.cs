using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;

namespace RemoteControl.Server
{
    public class HttpRequest
    {
        public string Method { get; set; }
        public string Url { get; set; }
        public string Path { get; set; }
        public string Content { get; set; }
        public NameValueCollection Query { get; set; }
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();
        public int ContentLength { get { return this.Headers.TryGetValue("Content-Length", out var str) && int.TryParse(str, out var value) ? value : 0; } }


        public HttpRequest(Stream stream)
        {
            this.readStream(stream);
        }


        /// <summary>
        /// Reads the stream
        /// </summary>
        private void readStream(Stream stream)
        {
            // initial line
            var tokens = readline(stream)?.Split(' ');
            if (tokens?.Length != 3)
                throw new Exception("Invalid http request header");

            this.Method = tokens[0].ToUpper();
            this.Url = tokens[1];
            this.Query = HttpUtility.ParseQueryString(this.Url.Contains("?") ? this.Url.Split('?')[1] : string.Empty);

            // reading headers
            string line;
            while (!string.IsNullOrEmpty(line = this.readline(stream)))
            {
                var header = line.Split(':');
                if (header.Length > 1)
                    this.Headers.Add(header[0], header[1].Trim());
            }

            // reading content
            if (this.ContentLength > 0)
                this.Content = Encoding.ASCII.GetString(new BinaryReader(stream).ReadBytes(this.ContentLength));
        }


        /// <summary>
        /// Reads a line from the stream
        /// </summary>
        private string readline(Stream stream)
        {
            var data = string.Empty;

            while (true)
            {
                var nextChar = stream.ReadByte();
                if (nextChar == '\n')
                    break;

                if (nextChar == '\r')
                    continue;

                if (nextChar == -1)
                    continue;

                data += Convert.ToChar(nextChar);
            }

            return data;
        }
    }
}
