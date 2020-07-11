using System;
using System.IO;
using RemoteControl.Server;
using TrayToolkit.Helpers;
using static RemoteControl.Server.HttpResponse;

namespace RemoteControl.Controllers
{
    public class FilesController : IController
    {
        private readonly string appVersion;
        private readonly string hostUrl;
        private readonly TimeSpan cacheAge = TimeSpan.FromDays(5 * 365);


        public FilesController(string appVersion, string hostUrl)
        {
            this.appVersion = appVersion;
            this.hostUrl = hostUrl;
        }


        /// <summary>
        /// Writes a file from the resources
        /// </summary>
        public void ProcessRequest(HttpContext context)
        {
            var path = context.Request.Url.Split('?')[0].Trim('/');
            var file = string.IsNullOrEmpty(path) ? "index.html" : path;
            using (var stream = this.getResource(file))
            {
                if (stream == null)
                    context.Response.StatusCode = System.Net.HttpStatusCode.NotFound;
                else
                {
                    context.Response.CacheAge = this.cacheAge;
                    if (Path.GetExtension(file) == ".html")
                        context.Response.Write(this.readFormattedHtml(stream));
                    else
                        context.Response.Write(stream, System.Web.MimeMapping.GetMimeMapping(file));
                }
            }
        }


        /// <summary>
        /// Returns the html file with evaluated variables
        /// </summary>
        private string readFormattedHtml(Stream s)
        {
            using (var r = new StreamReader(s))
                return r.ReadToEnd()
                    .Replace("{Version}", this.appVersion)
                    .Replace("{HostUrl}", this.hostUrl)
                    .Replace("{Title}", Environment.MachineName);
        }


        /// <summary>
        /// Returns a stream representinga file in resources
        /// </summary>
        private Stream getResource(string fileName)
        {
            var localFile = Path.Combine("../../App", fileName);
            if (File.Exists(localFile))
                return new FileStream(localFile, FileMode.Open, FileAccess.Read);

            return ResourceHelper.GetResourceStream("App." + fileName.Replace('/', '.'));
        }
    }
}
