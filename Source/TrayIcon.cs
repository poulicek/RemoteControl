using System.Diagnostics;
using System.Windows.Forms;
using RemoteControl.UI;
using TrayToolkit.UI;

namespace RemoteControl
{
    public partial class TrayIcon : TrayIconBase
    {
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
            using (var dlg = new MainForm(this.controller))
                dlg.ShowDialog();
            //Process.Start(this.controller.ServerUrl);
        }

        protected override void Dispose(bool disposing)
        {
            this.controller.Dispose();
            base.Dispose(disposing);
        }
    }
}
