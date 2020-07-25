using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using RemoteControl.Server;
using static TrayToolkit.Helpers.ResourceHelper;

namespace RemoteControl.Controllers
{
    public class ViewController : IController
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.CacheAge = TimeSpan.Zero;

            var view = context.Request.Query["v"];
            using (var s = this.getResource(view))
            {
                if (s != null)
                    this.writeView(context, s, view);
                else
                {
                    context.Response.StatusCode = HttpStatusCode.NotFound;
                    context.Response.Write($"The view '{view}' isn't available");
                }
            }
        }


        /// <summary>
        /// Writes the view
        /// </summary>
        private void writeView(HttpContext context, Stream s, string view)
        {
            switch (view)
            {
                case "combined":
                    context.Response.Write(FilesController.FillTemplate(s, new Dictionary<string, string>()
                    {
                        { "{View-Media}", this.getResource("media").ReadString() },
                        { "{View-Gamepad}", this.getResource("gamepad").ReadString() },
                    }));
                    break;

                default:
                    context.Response.Write(s);
                    break;
            }            
        }


        /// <summary>
        /// Returns a stream representing a file in resources
        /// </summary>
        private Stream getResource(string view)
        {
            return FilesController.GetResource($"Views/{view}.html");
        }
    }
}