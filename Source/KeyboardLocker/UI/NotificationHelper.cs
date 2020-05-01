using System;
using System.Drawing;
using System.Windows.Forms;

namespace KeyboardLocker.UI
{
    public partial class NotificationHelper
    {
        private static readonly Timer timer = new Timer();
        private static readonly BalloonTooltip instance = new BalloonTooltip();


        static NotificationHelper()
        {
            timer.Tick += onTimerTick;
        }


        /// <summary>
        /// Shows the notification
        /// </summary>
        public static void Show(Bitmap icon, string message, int timeout)
        {
            timer.Stop();
            timer.Interval = timeout;
            timer.Start();

            instance.Icon = icon;
            instance.Message = message;
            instance.Show();
        }


        /// <summary>
        /// Hides the notification
        /// </summary>
        public static void Hide()
        {
            timer.Stop();
            instance.Hide();
        }

        private static void onTimerTick(object sender, EventArgs e)
        {
            Hide();
        }
    }
}
