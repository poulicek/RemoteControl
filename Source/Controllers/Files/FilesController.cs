using System;
using System.Collections.Generic;
using System.IO;
using RemoteControl.Server;
using TrayToolkit.Helpers;

namespace RemoteControl.Controllers.Files
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
            using (var stream = GetResource(file))
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
            return FillTemplate(s.ReadString(), new Dictionary<string, string>()
            {
                { "{TimeStamp}", DateTime.UtcNow.Ticks.ToString() },
                { "{Version}", this.appVersion },
                { "{HostUrl}", this.hostUrl },
                { "{Title}", Environment.MachineName },
            });
        }


        /// <summary>
        /// Fills the template with variables
        /// </summary>
        internal static string FillTemplate(string str, Dictionary<string, string> variables)
        {
            foreach (var v in variables)
                str = str.Replace(v.Key, v.Value);

            return str;
        }


        /// <summary>
        /// Returns a stream representing a file in resources
        /// </summary>
        internal static Stream GetResource(string fileName)
        {
            var localFile = Path.Combine("../../App", fileName);
            if (File.Exists(localFile))
                return new FileStream(localFile, FileMode.Open, FileAccess.Read);

            return ResourceHelper.GetResourceStream("App." + fileName.Replace('/', '.'));
        }
    }
}
