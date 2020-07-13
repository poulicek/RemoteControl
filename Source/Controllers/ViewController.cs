using System;
using System.IO;
using System.Net;
using RemoteControl.Server;
using TrayToolkit.Helpers;

namespace RemoteControl.Controllers
{
    public class ViewController : IController
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.CacheAge = TimeSpan.Zero;

            var view = context.Request.Query["v"];
            using (var res = this.getResource(view))
            {
                if (res != null)
                    context.Response.Write(res);
                else
                {
                    context.Response.StatusCode = HttpStatusCode.NotFound;
                    context.Response.Write($"The view '{view}' isn't available");
                }
            }
        }

        /// <summary>
        /// Returns a stream representing a file in resources
        /// </summary>
        private Stream getResource(string view)
        {
            var localFile = Path.Combine("../../App/Views", view + ".html");
            if (File.Exists(localFile))
                return new FileStream(localFile, FileMode.Open, FileAccess.Read);

            return ResourceHelper.GetResourceStream($"App.Views.{view}.html");
        }
    }
}