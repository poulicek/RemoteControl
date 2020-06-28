using System.Windows.Forms;
using TrayToolkit.UI;

namespace RemoteControl.UI
{
    public partial class TrayIcon : TrayIconBase
    {
        private MainForm dialog;
        private readonly Controller controller = new Controller();

        public TrayIcon() : base("Remote Control", "https://github.com/poulicek/RemoteControl")
        {
        }

        protected override string getIconName(bool lightMode)
        {
            return lightMode
                ? "Resources.IconInactiveLight.png"
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
            this.controller.Dispose();
            base.Dispose(disposing);
        }
    }
}
