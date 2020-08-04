using System;
using System.Drawing;
using System.Windows.Forms;
using RemoteControl.Controllers;
using TrayToolkit.Helpers;
using TrayToolkit.UI;

namespace RemoteControl.UI
{
    public partial class TrayIcon : TrayIconBase
    {
        private MainForm dialog;
        private readonly Bitmap tooltipIcon = ResourceHelper.GetResourceImage("Resources.IconDark.png");
        private readonly MainController controller = new MainController();

        public TrayIcon() : base("Remote Control", "https://github.com/poulicek/RemoteControl")
        {
            this.controller.ConnectionError += this.onConnectionError;
            this.controller.StartServer();
        }

        private void onConnectionError(Exception ex)
        {
            BalloonTooltip.Show("Network connection not available", this.tooltipIcon, ex.Message);
        }

        protected override string getIconName(bool lightMode)
        {
            return lightMode
                ? "Resources.IconLight.png"
                : "Resources.IconDark.png";
        }

        protected override void onTrayIconClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            if (this.dialog == null || this.dialog.IsDisposed)
                this.dialog = new MainForm(this.controller);
            
            this.dialog.Show();
            this.dialog.WindowState = FormWindowState.Normal;
            this.dialog.Activate();
        }

        protected override void Dispose(bool disposing)
        {
            this.tooltipIcon.Dispose();
            this.controller.Dispose();
            base.Dispose(disposing);
        }
    }
}
