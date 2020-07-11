﻿using System.Windows.Forms;
using RemoteControl.Server;
using TrayToolkit.Helpers;

namespace RemoteControl.Controllers
{
    public class KeysController : IController
    {
        public void ProcessRequest(HttpContext context)
        {
            if (!int.TryParse(context.Request.Query["v"], out var keyCode))
                return;

            if (!int.TryParse(context.Request.Query["s"], out var keyState) || keyState == 1)
                ((Keys)keyCode).Down();
            else
                ((Keys)keyCode).Up();
        }
    }
}
