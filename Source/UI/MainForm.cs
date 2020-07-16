using System;
using System.Drawing;
using System.Windows.Forms;
using QRCoder;
using RemoteControl.Controllers;
using TrayToolkit.Helpers;

namespace RemoteControl.UI
{
    public partial class MainForm : Form
    {
        private readonly Bitmap qrImage;

        public MainForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }

        public MainForm(MainController controller) : this()
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
