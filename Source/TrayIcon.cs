using System;
using System.Windows.Forms;
using Common;

namespace RemoteControl
{
    public partial class TrayIcon : TrayIconBase
    {
        private readonly CommandController commandController = new CommandController();

        public TrayIcon() : base("Remote Control")
        {
        }

        protected override string getIconName(bool lightMode)
        {
            return null;
        }

        protected override void onTrayIconClick(object sender, MouseEventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            this.commandController.Dispose();
            base.Dispose(disposing);
        }
    }
}
