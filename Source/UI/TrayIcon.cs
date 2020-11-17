using System;
using System.Drawing;
using System.Windows.Forms;
using RemoteControl.Logic;
using TrayToolkit.Helpers;
using TrayToolkit.UI;

namespace RemoteControl.UI
{
    public class TrayIcon : TrayIconBase
    {
        private MainForm dialog;
        private readonly Bitmap tooltipIcon = ResourceHelper.GetResourceImage("Resources.IconDark.png");
        private readonly InputListener listener = new InputListener();

        public TrayIcon() : base("Remote Control", "https://github.com/poulicek/RemoteControl")
        {
            this.listener.ConnectedChanged += this.onConnectedChanged;
            this.listener.ConnectionError += this.onConnectionError;
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.listener.InitServer();
        }


        #region State Event Handlers

        private void onConnectedChanged(bool connected)
        {
            this.updateLook();
        }

        private void onConnectionError(Exception ex)
        {
            BalloonTooltip.Show("Network connection not available", this.tooltipIcon, ex.Message, 5000);
        }

        #endregion

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

        protected override void onTrayIconClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (!this.listener.IsConnected)
                this.listener.InitServer();

            if (this.dialog?.Visible == true && this.dialog.WindowState == FormWindowState.Normal)
                this.dialog.Close();
            else
            {
                if (this.dialog == null || this.dialog.IsDisposed)
                    this.dialog = new MainForm(this.listener);

                this.dialog.Show();
                this.dialog.WindowState = FormWindowState.Normal;
                this.dialog.Activate();
            }
        }

        protected override void Dispose(bool disposing)
        {
            this.tooltipIcon.Dispose();
            this.listener.Dispose();
            base.Dispose(disposing);
        }
    }
}
