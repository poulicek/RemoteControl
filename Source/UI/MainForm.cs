using System;
using System.Windows.Forms;
using RemoteControl.Logic;

namespace RemoteControl.UI
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }

        public MainForm(MainLoop controller) : this()
        {
            this.webBrowser.ProgressChanged += this.onProgressChanged;
            this.webBrowser.Url = new Uri(controller.ServerUrl + "?v=" + controller.AppVersion + "#link");
        }

        private void onProgressChanged(object sender, WebBrowserProgressChangedEventArgs e)
        {
            if (e.CurrentProgress == e.MaximumProgress && !this.webBrowser.Visible)
                this.webBrowser.Visible = true;
        }
    }
}
