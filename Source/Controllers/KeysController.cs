﻿using System;
using System.Windows.Forms;
using RemoteControl.Server;
using TrayToolkit.Helpers;

namespace RemoteControl.Controllers
{
    public class KeysController : IController
    {
        public void ProcessRequest(HttpContext context)
        {
            if (!int.TryParse(context.Request.Query["v"], out var keyCode))
            {
                if (!Enum.TryParse<Keys>(context.Request.Query["r"], true, out var key))
                    return;

                keyCode = (int)key;
            }


            var scanMode = context.Request.Query["a"] == "1";
            if (int.TryParse(context.Request.Query["s"], out var keyState) && keyState == 0)
                ((Keys)keyCode).Up(scanMode);
            else
                ((Keys)keyCode).Down(scanMode);
        }
    }
}
