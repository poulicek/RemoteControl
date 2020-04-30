using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace KeyboardLocker.UI
{
    public partial class NotificationHelper
    {
        private static readonly Timer timer = new Timer();
        private static readonly Form instance = new BalloonForm();


        static NotificationHelper()
        {
            timer.Tick += onTimerTick;
        }

        public static void Show(string message, int timeout)
        {
            timer.Stop();
            timer.Interval = timeout;
            timer.Start();

            instance.Show();
        }

        public static void Hide()
        {
            timer.Stop();
            instance.Hide();
        }

        private static void onTimerTick(object sender, EventArgs e)
        {
            Hide();
        }


        private class BalloonForm : Form
        {
            public BalloonForm()
            {
                this.BackColor = Color.Black;// Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(36)))), ((int)(((byte)(36)))));
                this.ClientSize = new Size(364, 102);
                this.FormBorderStyle = FormBorderStyle.None;
                this.ShowInTaskbar = false;
                this.TopMost = true;
                this.StartPosition = FormStartPosition.CenterScreen;
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(160, 0, 0, 0)), this.DisplayRectangle);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
            }
        }
    }
}
