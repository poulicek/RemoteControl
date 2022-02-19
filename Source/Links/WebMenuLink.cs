using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;
using RemoteControl.Logic;
using TrayToolkit.Helpers;

namespace RemoteControl.Links
{
    public class WebMenuLink : MenuLinkBase
    {
        public const int HTTP_TIMEOUT = 10000;
        public const int PREFERED_ICON_SIZE = 32;

        public WebMenuLink(string file) : base(file)
        {
        }


        public WebMenuLink(string name, string link) : base(name, link)
        {
        }


        /// <summary>
        /// Reads the web link file
        /// </summary>
        protected override bool ReadFile(string file)
        {
            try
            {
                this.Link = IniHelper.ReadFile(file)["URL"];
                return !string.IsNullOrEmpty(this.Link);
            }
            catch { return false; }
            
        }


        /// <summary>
        /// Returns the icon
        /// </summary>
        public override byte[] GetIcon(out string mime)
        {
            var icon = this.parseIconFromHtml(this.Link);
            if (string.IsNullOrEmpty(icon))
            {
                mime = null;
                return null;
            }

            mime = MimeMapping.GetMimeMapping(icon);
            return this.downloadIcon(icon);
        }


        /// <summary>
        /// Launches the web link
        /// </summary>
        public override void Launch()
        {
            // commented as use of Process.Start is restricted on Microsoft Store
            //Process.Start(this.Link);
        }



        /// <summary>
        /// Returns the icon URL parsed from HTML document
        /// </summary>
        private string parseIconFromHtml(string url)
        {
            string html;
            var lastSize = 0;
            var lastIcon = string.Empty;

            // downloading the HTML
            using (var wc = new TimedWebClient(HTTP_TIMEOUT))
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

                if (!string.IsNullOrEmpty(iconHref) && (lastSize == 0 || size == PREFERED_ICON_SIZE))
                {
                    lastSize = size;
                    lastIcon = iconHref;
                }
            }

            return lastIcon;
        }



        /// <summary>
        /// Downloads the icon
        /// </summary>
        private byte[] downloadIcon(string url)
        {
            using (var wc = new TimedWebClient(HTTP_TIMEOUT))
            {
                wc.Headers.Add("User-Agent: Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.198 Mobile Safari/537.36 Edg/86.0.622.69");
                return wc.DownloadData(url);
            }
        }
    }
}
