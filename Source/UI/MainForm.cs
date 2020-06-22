using System;
using System.Drawing;
using System.Windows.Forms;
using QRCoder;
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

        public MainForm(Controller controller) : this()
        {
            this.webBrowser.ProgressChanged += this.onProgressChanged;
            this.webBrowser.Url = new Uri(controller.ServerUrl);
            this.qrImage = this.getQRCode(controller.ServerUrl);
            this.Location = this.GetCornerLocation();
        }

        private void onProgressChanged(object sender, WebBrowserProgressChangedEventArgs e)
        {
            if (e.CurrentProgress == e.MaximumProgress)
                this.webBrowser.Visible = true;
        }

        private Bitmap getQRCode(string str)
        {
            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(str, QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new QRCode(qrCodeData))
                return qrCode.GetGraphic(48, Color.White, this.qrBox.BackColor, true);
        }

        private void qrBox_Paint(object sender, PaintEventArgs e)
        {
            //this.btnClose.Visible = true;

            if (this.qrImage == null)
                return;

            var m = 32;
            var w = this.qrBox.Width - m - m;
            var h = this.qrImage.Height * w / this.qrImage.Width;
            e.Graphics.DrawImage(this.qrImage, new Rectangle(m, (this.qrBox.Height - h) / 2, w, h), new Rectangle(0, 0, this.qrImage.Width, this.qrImage.Height), GraphicsUnit.Pixel);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
