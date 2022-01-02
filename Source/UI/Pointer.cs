using System;
using System.Drawing;
using System.Windows.Forms;
using TrayToolkit.Helpers;

namespace RemoteControl.UI
{
    public partial class Pointer : Form
    {
        private const int SIZE = 50;
        private const int ANIMATION_TIME = 100;

        private static Pointer instance;

        private DateTime firstPaint;

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
            this.DoubleBuffered = true;

            this.ResumeLayout(false);
        }

        protected override void OnLoad(EventArgs e)
        {
            this.Size = new Size(SIZE, SIZE);
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            var now = DateTime.Now;
            if (firstPaint == DateTime.MinValue)
                firstPaint = now;

            var ratio = Math.Min((now - firstPaint).TotalMilliseconds / ANIMATION_TIME, 1);
            var w = (int)(ratio * this.Width);
            var h = (int)(ratio * this.Height);
            var rect = new Rectangle(this.Width / 2 - w / 2, this.Height / 2 - h / 2, w - 1, h - 1);

            this.drawPointer(e.Graphics, Color.DeepPink.SetAlphaChannel((byte)Math.Ceiling(128 * ratio)), rect);
        }


        /// <summary>
        /// Draws the shape of the pointer
        /// </summary>
        private void drawPointer(Graphics g, Color c, Rectangle rect)
        {
            using (var b = new SolidBrush(c))
            {
                g.FillEllipse(b, rect);
                g.DrawEllipse(Pens.DeepPink, rect);
            }
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
                instance.Refresh();
            });
        }


        /// <summary>
        /// Hides the pointer
        /// </summary>
        public static void HidePointer()
        {
            TrayIcon.Invoke(() =>
            {
                instance?.Dispose();
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
