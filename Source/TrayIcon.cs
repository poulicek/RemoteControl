using System.Windows.Forms;
using RemoteControl.UI;
using TrayToolkit.UI;

namespace RemoteControl
{
    public partial class TrayIcon : TrayIconBase
    {
        private MainForm dialog;
        private readonly Controller controller = new Controller();

        public TrayIcon() : base("Remote Control")
        {
        }

        protected override string getIconName(bool lightMode)
        {
            return lightMode
                ? "Resources.IconLight.png"
                : "Resources.IconDark.png";
        }

        protected override void onTrayIconClick(object sender, MouseEventArgs e)
        {
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
