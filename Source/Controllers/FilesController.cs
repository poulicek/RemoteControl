using System.IO;
using RemoteControl.Server;
using TrayToolkit.Helpers;
using static RemoteControl.Server.HttpResponse;

namespace RemoteControl.Controllers
{
    public class FilesController : IController
    {
        private readonly string appVersion;

        public FilesController(string appVersion)
        {
            this.appVersion = appVersion;
        }


        /// <summary>
        /// Writes a file from the resources
        /// </summary>
        public void ProcessRequest(HttpContext context)
        {
            var path = context.Request.Url.Trim('/');
            var file = string.IsNullOrEmpty(path) ? "index.html" : path;
            var stream = this.getResource(file);

            if (stream == null)
                context.Response.StatusCode = System.Net.HttpStatusCode.NotFound;
            else
            {
                context.Response.Cache = CacheControl.NoCache;
                if (Path.GetExtension(file) == ".html")
                    context.Response.Write(this.convertToHtml(stream));
                else
                    context.Response.Write(stream, System.Web.MimeMapping.GetMimeMapping(file));
            }
        }


        /// <summary>
        /// Returns the html file with evaluated variables
        /// </summary>
        private string convertToHtml(Stream s)
        {
            using (var r = new StreamReader(s))
            {
                var html = r.ReadToEnd();
                html = html.Replace("{Version}", this.appVersion);
                html = html.Replace("{Title}", "Remote Control");
                return html;
            }
        }


        /// <summary>
        /// Returns a stream representinga file in resources
        /// </summary>
        private Stream getResource(string fileName)
        {
            var localFile = Path.Combine("../../App", fileName);
            if (File.Exists(localFile))
                return new FileStream(localFile, FileMode.Open, FileAccess.Read);

            return ResourceHelper.GetResourceStream("RemoteControl.App." + fileName.Replace('/', '.'));
        }
    }
}
