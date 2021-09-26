using System;
using RemoteControl.Links;
using RemoteControl.Server;

namespace RemoteControl.Controllers.Menu
{
    public class MenuController : IController
    {
        private readonly WebMenuLink[] links = new WebMenuLink[] { new WebMenuLink("Jango", "https://www.jango.com/"), new WebMenuLink("Spotify", "https://open.spotify.com/") };

        public void ProcessRequest(HttpContext context)
        {
            if (!int.TryParse(context.Request.Query["v"], out var linkId))
                return;

            var link = linkId >= 0 && linkId < this.links.Length ? this.links[linkId] : null;
            if (link == null)
                return;

            if (context.Request.Query["r"] == "icon")
                this.writeIcon(context, link.GetIcon(out var mime), mime);
            else
                link.Launch();
        }



        /// <summary>
        /// Writes the icon to response stream
        /// </summary>
        private void writeIcon(HttpContext context, byte[] iconData, string mime)
        {
            if (iconData != null)
            {
                context.Response.CacheAge = TimeSpan.FromDays(365);
                context.Response.Write(iconData, mime);
            }
        }
    }
}
