﻿using System;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using RemoteControl.Server;
using TrayToolkit.Helpers;

namespace RemoteControl.Controllers.Media
{
    public class MediaController : IController, IDisposable
    {
        private readonly TrayToolkit.OS.Display.DisplayController display = new TrayToolkit.OS.Display.DisplayController();

        public void ProcessRequest(HttpContext context)
        {
            switch (context.Request.Query["v"])
            {
                case "brightnessUp":
                    this.display.SetBrightness(this.display.CurrentValue + 10);
                    break;

                case "brightnessDown":
                    this.display.SetBrightness(this.display.CurrentValue - 10);
                    break;

                case "screenOff":
                    this.display.TurnOff();
                    break;

                case "standby":
                    this.setSuppendStateAsync();
                    break;

                default:
                    if (int.TryParse(context.Request.Query["v"], out var keyCode))
                        ((Keys)keyCode).Down();
                    break;
            }
        }


        private void setSuppendStateAsync()
        {
            // reliable way to put PC to sleep is by setting the SuspendState 1 which, however, uses hibarnation if it is enabled
            if (Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Power", false).GetValue("HibernateEnabled") as int? == 0)
                Application.SetSuspendState(PowerState.Hibernate, true, true);
            else
            {
                var time = DateTime.Now;

                // turning of the display triggers sleep if S0 power state is supported
                this.display.TurnOff();

                ThreadingHelper.DoAsync(() =>
                {
                    // giving a chance the computer to go to sleep
                    Thread.Sleep(3000);

                    // turning on standby if the computer remained active while the screen was off
                    if ((DateTime.Now - time).TotalSeconds < 5)
                        Application.SetSuspendState(PowerState.Suspend, true, true);
                });
            }
        }


        public void Dispose()
        {
            this.display.Dispose();
        }
    }
}
