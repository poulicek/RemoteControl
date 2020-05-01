using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KeyboardLocker.UI
{
    public class BalloonTooltip : IDisposable
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

        private class UnfocusedForm : Form { protected override bool ShowWithoutActivation => true;}

        private readonly UnfocusedForm form;
        private readonly StringFormat sf = new StringFormat() { LineAlignment = StringAlignment.Center };

        public Bitmap Icon { get; set; }
        public string Message { get; set; }

        public BalloonTooltip()
        {
            this.form = new UnfocusedForm()
            {
                BackColor = Color.FromArgb(36, 36, 36),
                ClientSize = new Size(364, 102),
                FormBorderStyle = FormBorderStyle.None,
                ShowInTaskbar = false,
                TopMost = true,
                StartPosition = FormStartPosition.Manual,
            };

            this.form.Paint += this.onFormPaint;
        }

        #region Event Handlers

        private void onFormPaint(object sender, PaintEventArgs e)
        {
            this.drawContents(e.Graphics);
        }

        #endregion

        #region Interface

        public void Show()
        {
            var loc = this.getCornerLocation();
            ShowWindow(this.form.Handle, SW_SHOWNOACTIVATE);
            SetWindowPos(this.form.Handle.ToInt32(), 0, loc.X, loc.Y, this.form.Width, this.form.Height, SWP_NOACTIVATE);
        }

        public void Hide()
        {
            this.form.Hide();
        }

        public void Dispose()
        {
            this.form.Dispose();
        }

        #endregion

        #region UI Drawing

        /// <summary>
        /// Sets the desired location
        /// </summary>
        private Point getCornerLocation()
        {
            var screenArea = Screen.GetWorkingArea(this.form);
            return new Point(screenArea.Width - 24 - this.form.Width, screenArea.Height - 16 - this.form.Height);
        }

        /// <summary>
        /// Draws the contents
        /// </summary>
        /// <param name="g"></param>
        private void drawContents(Graphics g)
        {
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;

            var textRect = Rectangle.Inflate(this.form.ClientRectangle, -16, -16);
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
}
