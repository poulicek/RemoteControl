using System;
using System.Drawing;
using System.Windows.Forms;

namespace RemoteControl.UI
{
    public partial class Pointer : Form
    {
        private const int SIZE = 25;

        private static Pointer instance;

        public Pointer()
        {
            this.SuspendLayout();

            this.TopMost = true;
            this.AutoScaleMode = AutoScaleMode.None;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.White;
            this.TransparencyKey = this.BackColor;
            this.Opacity = 0.4;
            this.ShowInTaskbar = false;

            this.ResumeLayout(false);
        }

        protected override void OnLoad(EventArgs e)
        {
            this.Size = new Size(SIZE, SIZE);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.FillEllipse(Brushes.DeepPink, 0, 0, this.Width - 1, this.Height - 1);
        }


        #region Static Interface

        /// <summary>
        /// Shows the pointer at given position
        /// </summary>
        public static void ShowPointer(Point point)
        {
            TrayIcon.Invoke(() =>
            {
                if (instance == null)
                {
                    instance = new Pointer();
                    instance.Show();
                }

                point.Offset(-instance.Width / 2, -instance.Height / 2);
                instance.Location = point;
            });
        }


        /// <summary>
        /// Hides the pointer
        /// </summary>
        public static void HidePointer()
        {
            TrayIcon.Invoke(() =>
            {
                instance?.Close();
                instance = null;
            });
        }

        #endregion

        #region Form Transparency

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x00000020; //WS_EX_TRANSPARENT
                return cp;
            }
        }


        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x0084;
            const int HTTRANSPARENT = (-1);

            if (m.Msg == WM_NCHITTEST)
            {
                m.Result = (IntPtr)HTTRANSPARENT;
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        #endregion
    }
}
