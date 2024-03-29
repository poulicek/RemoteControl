﻿using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using RemoteControl.Logic;
using TrayToolkit.Helpers;
using TrayToolkit.UI;

namespace RemoteControl.UI
{
    public class TrayIcon : TrayIconBase
    {
        private static TrayIcon instance;

        private MainForm dialog;
        private readonly Bitmap tooltipIcon = ResourceHelper.GetResourceImage("Resources.IconDark.png");
        private readonly RequestHandler listener = new RequestHandler();

        public TrayIcon() : base("Simple Remote Control", "https://github.com/poulicek/RemoteControl", false)
        {
            instance = this;
            this.listener.NotificationRaised += this.onNotificationRaised;
            this.listener.ConnectedChanged += this.onConnectedChanged;
            this.listener.ConnectionError += this.onConnectionError;
            SystemEvents.SessionSwitch += this.onSessionSwitch;
        }


        internal static void Invoke(Action callback)
        {
            instance?.BeginInvoke(callback);
        }


        protected override string getIconName(bool lightMode)
        {
            return lightMode
                ? "Resources.IconLight.png"
                : "Resources.IconDark.png";
        }

        protected override Icon getIconFromBitmap(Bitmap bmp)
        {
            if (this.listener.IsConnected)
                return base.getIconFromBitmap(bmp);

            using (bmp)
                return base.getIconFromBitmap(bmp.MakeGrayscale());
        }

        private void showMainDialog()
        {
            if (this.dialog == null || this.dialog.IsDisposed)
                this.dialog = new MainForm(this.listener);

            this.dialog.Show();
            this.dialog.WindowState = FormWindowState.Normal;
            this.dialog.Activate();
        }


        private void handleFirstStart()
        {
            try
            {
                if (AppConfigHelper.Get<bool>("Initialized"))
                    return;

                AppConfigHelper.Set("Initialized", true);
                this.showMainDialog();
            }
            catch { }
        }

        #region Event Handlers

        private void onSessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (!this.listener.IsConnected && (e.Reason == SessionSwitchReason.SessionLogon || e.Reason == SessionSwitchReason.SessionUnlock))
                this.listener.StartServer();
        }

        private void onNotificationRaised(string msg)
        {
            BalloonTooltip.Show(msg, this.tooltipIcon, null, 5000);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.listener.StartServer();
            this.handleFirstStart();
        }

        private void onConnectedChanged(bool connected)
        {
            this.updateLook();
        }

        private void onConnectionError(Exception ex)
        {
            BalloonTooltip.Show("Network connection not available", this.tooltipIcon, ex.Message, 5000);
        }

        protected override void onTrayIconClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (!this.listener.IsConnected)
                this.listener.StartServer();

            if (this.dialog?.Visible == true && this.dialog.WindowState == FormWindowState.Normal)
                this.dialog.Close();
            else
                this.showMainDialog();
        }

        #endregion


        protected override void Dispose(bool disposing)
        {
            SystemEvents.SessionSwitch -= this.onSessionSwitch;
            this.tooltipIcon.Dispose();
            this.listener.Dispose();
            base.Dispose(disposing);
        }
    }
}
