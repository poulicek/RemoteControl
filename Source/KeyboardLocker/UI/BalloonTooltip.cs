using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KeyboardLocker.UI
{
    public static class BalloonTooltip
    {
        #region Balloon Control

        private class BalloonControl : Form
        {
            #region P/Invoke

            private const int SW_SHOWNOACTIVATE = 4;
            private const int HWND_TOPMOST = -1;
            private const uint SWP_NOACTIVATE = 0x10;

            [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
            private static extern bool SetWindowPos(int hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

            [DllImport("user32.dll")]
            private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

            #endregion

            private readonly StringFormat sf = new StringFormat() { LineAlignment = StringAlignment.Center };

            public new Bitmap Icon { get; set; }
            public string Message { get; set; }

            protected override bool ShowWithoutActivation => true;

            public BalloonControl()
            {
                this.BackColor = Color.FromArgb(36, 36, 36);
                this.ClientSize = new Size(364, 102);
                this.FormBorderStyle = FormBorderStyle.None;
                this.ShowInTaskbar = false;
                this.TopMost = true;
                this.StartPosition = FormStartPosition.Manual;
                this.DoubleBuffered = true;
            }

            /// <summary>
            /// Gets the desired location
            /// </summary>
            private Point getCornerLocation()
            {
                var screenArea = Screen.GetWorkingArea(this);
                return new Point(screenArea.Width - 24 - this.Width, screenArea.Height - 16 - this.Height);
            }


            /// <summary>
            /// Shows the unfocused contorl
            /// </summary>
            public void ShowUnfocused()
            {
                var loc = this.getCornerLocation();
                ShowWindow(this.Handle, SW_SHOWNOACTIVATE);
                SetWindowPos(this.Handle.ToInt32(), 0, loc.X, loc.Y, this.Width, this.Height, SWP_NOACTIVATE);
                this.Invalidate();
            }

            #region Look

            protected override void OnPaint(PaintEventArgs e)
            {
                this.drawContents(e.Graphics);
            }

            /// <summary>
            /// Draws the contents
            /// </summary>
            private void drawContents(Graphics g)
            {
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;

                var textRect = Rectangle.Inflate(this.ClientRectangle, -16, -16);
                var iconRect = this.drawIcon(g, textRect);
                var textMargin = iconRect.IsEmpty ? 0 : iconRect.Width + 16;

                this.drawMessage(g, textRect, textMargin);
            }


            /// <summary>
            /// Draws the message
            /// </summary>
            private Rectangle drawMessage(Graphics g, Rectangle rect, int margin)
            {
                if (string.IsNullOrEmpty(this.Message))
                    return Rectangle.Empty;

                rect.X += margin;
                rect.Width -= margin;

                using (var fontpath = new GraphicsPath())
                using (var fontFamily = new FontFamily("Segoe UI"))
                {
                    fontpath.AddString(this.Message, fontFamily, (int)FontStyle.Regular, 16, rect, this.sf);
                    g.FillPath(Brushes.White, fontpath);
                }

                return rect;
            }


            /// <summary>
            /// Draws the icon
            /// </summary>
            private Rectangle drawIcon(Graphics g, Rectangle rect)
            {
                if (this.Icon == null)
                    return Rectangle.Empty;

                var height = rect.Height - 32;
                var width = this.Icon.Width * height / this.Icon.Height;
                var iconRect = new Rectangle(rect.X, rect.Y + 16, width, height);

                g.DrawImage(this.Icon, iconRect);

                return iconRect;
            }

            #endregion
        }

        #endregion

        private static readonly Timer timer = new Timer();
        private static readonly BalloonControl tooltip = new BalloonControl();

        static BalloonTooltip()
        {
            timer.Tick += onTimerTick;
        }


        /// <summary>
        /// Resets the timer
        /// </summary>
        private static void resetTimer(int timeout = 0)
        {
            timer.Stop();

            if (timeout > 0)
            {
                timer.Interval = timeout;
                timer.Start();
            }
        }


        /// <summary>
        /// Shows the notification
        /// </summary>
        public static void Show(Bitmap icon, string message, int timeout = 0)
        {
            resetTimer(timeout);

            tooltip.Icon = icon;
            tooltip.Message = message;
            tooltip.ShowUnfocused();
        }


        /// <summary>
        /// Hides the notification
        /// </summary>
        public static void Hide()
        {
            resetTimer();
            tooltip.Hide();
        }

        private static void onTimerTick(object sender, EventArgs e)
        {
            tooltip.Hide();
        }
    }
}
