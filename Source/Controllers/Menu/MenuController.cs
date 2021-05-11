using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using RemoteControl.Server;

namespace RemoteControl.Controllers.Menu
{
    public class MenuController : IController
    {
        private readonly string[] links = new string[] { "https://www.jango.com/", "https://open.spotify.com/" };
        private readonly Dictionary<int, Process> runningProcesses = new Dictionary<int, Process>();

        public void ProcessRequest(HttpContext context)
        {
            if (!int.TryParse(context.Request.Query["v"], out var linkId))
                return;

            var link = linkId >= 0 && linkId < this.links.Length ? this.links[linkId] : null;
            if (string.IsNullOrEmpty(link))
                return;

            if (context.Request.Query["r"] == "icon")
                this.writeIcon(context, link);
            else
                this.gotoLink(linkId, link);
        }

        #region Link Execution

        /// <summary>
        /// Starts the process
        /// </summary>
        private void gotoLink(int id, string link)
        {
            if (link.StartsWith("http"))
                Process.Start(link);
            else if (!this.runningProcesses.TryGetValue(id, out var p) || p.HasExited)
                runningProcesses[id] = Process.Start(link);
        }

        #endregion

        #region Icon Processing

        /// <summary>
        /// Returns the icon URL parsed from HTML document
        /// </summary>
        private string parseIconFromHtml(string url, int preferedSize = 32)
        {
            string html;
            var lastSize = 0;
            var lastIcon = string.Empty;

            // downloading the HTML
            using (var wc = new WebClient())
            {
                wc.Headers.Add("User-Agent: Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.198 Mobile Safari/537.36 Edg/86.0.622.69");
                html = wc.DownloadString(url).ToLower();
            }

            // finding the icons
            foreach (var m in Regex.Matches(html, "<link[^>]+rel=\"icon\"[^>]+>"))
            {
                var link = m.ToString();
                var iconHref = Regex.Match(link, "href=\"([^\"]+)\"")?.Groups[1].Value;
                var iconSize = Regex.Match(link, "sizes=\"([0-9]+)x[0-9]+\"")?.Groups[1].Value;

                int.TryParse(iconSize, out var size);

                if (!string.IsNullOrEmpty(iconHref) && (lastSize == 0 || size == preferedSize))
                {
                    lastSize = size;
                    lastIcon = iconHref;
                }
            }

            return lastIcon;
        }



        /// <summary>
        /// Writes the icon to response stream
        /// </summary>
        private void writeIcon(HttpContext context, string url)
        {
            var icon = this.parseIconFromHtml(url);
            if (!string.IsNullOrEmpty(icon))
            {
                context.Response.CacheAge = TimeSpan.FromDays(365);
                context.Response.Write(this.downloadIcon(icon), context.Response.GetMime(Path.GetExtension(icon)));
            }
        }


        /// <summary>
        /// Downloads the icon
        /// </summary>
        private byte[] downloadIcon(string url)
        {
            using (var wc = new WebClient())
            {
                wc.Headers.Add("User-Agent: Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.198 Mobile Safari/537.36 Edg/86.0.622.69");
                return wc.DownloadData(url);
            }
        }

        #endregion
    }
}
