using System;
using System.Net;

namespace RemoteControl.Logic
{
    internal class TimedWebClient : WebClient
    {
        private readonly int timeoutMs;


        public TimedWebClient(int timeoutMs)
        {
            this.timeoutMs = timeoutMs;
        }


        protected override WebRequest GetWebRequest(Uri uri)
        {
            var w = base.GetWebRequest(uri);
            w.Timeout = this.timeoutMs;
            return w;
        }


        public bool UrlExists(string url)
        {
            try
            {
                this.DownloadString(url);
                return true;
            }
            catch { return false; }
        }
    }
}
