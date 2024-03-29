﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;

namespace RemoteControl.Server
{
    public class HttpRequest
    {
        public bool IsGZipAccepted { get { return this.GetHeader<string>("Accept-Encoding")?.StartsWith("gzip", StringComparison.InvariantCultureIgnoreCase) == true; } }
        public string Method { get; set; }
        public string Url { get; set; }
        public string Content { get; set; }
        public NameValueCollection Query { get; set; }
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();
        public int ContentLength { get { return this.GetHeader<int>("Content-Length"); } }


        public HttpRequest(Stream stream)
        {
            this.readStream(stream);
        }


        /// <summary>
        /// Returns the header
        /// </summary>
        public T GetHeader<T>(string header)
        {
            return this.Headers.TryGetValue(header, out var str) ? (T)Convert.ChangeType(str, typeof(T)) : default(T);
        }


        /// <summary>
        /// Reads the stream
        /// </summary>
        private void readStream(Stream stream)
        {
            // initial line
            var reader = new StreamReader(stream);
            var line = reader.ReadLine();
            var tokens = line?.Split(' ');
            if (tokens?.Length != 3)
                throw new FormatException("Invalid http request header: " + line);

            this.Method = tokens[0].ToUpper();
            this.Url = tokens[1];
            this.Query = HttpUtility.ParseQueryString(this.Url.Contains("?") ? this.Url.Split('?')[1] : string.Empty);

            // reading headers
            while (!string.IsNullOrEmpty(line = reader.ReadLine()))
            {
                var header = line.Split(':');
                if (header.Length > 1)
                    this.Headers.Add(header[0], header[1].Trim());
            }

            // reading content
            if (this.ContentLength > 0)
                this.Content = Encoding.ASCII.GetString(new BinaryReader(stream).ReadBytes(this.ContentLength));
        }
    }
}
