﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using RemoteControl.Server;
using static TrayToolkit.Helpers.ResourceHelper;

namespace RemoteControl.Controllers
{
    public class ViewController : IController
    {
        private readonly string serverUrl;

        public ViewController(string serverUrl)
        {
            this.serverUrl = serverUrl;
        }


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
                case "main":
                    context.Response.Write(FilesController.FillTemplate(s, new Dictionary<string, string>()
                    {
                        { "{View-Portrait}", this.getResource("media").ReadString() },
                        { "{View-Landscape}", this.getResource("rdp").ReadString() },
                    }));
                    break;

                case "link":
                    context.Response.Write(FilesController.FillTemplate(s, new Dictionary<string, string>()
                    {
                        { "{Link}", this.serverUrl },
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