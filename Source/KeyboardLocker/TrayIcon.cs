using System;
using System.Diagnostics;
using System.Drawing;
using System.Media;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace KeyboardLocker
{    public class TrayIcon : Form
    {
        private DateTime lastKeyBlockedNotification;
        private NotifyIcon trayIcon;
        private readonly InputBlocker inputBlocker;


        public TrayIcon()
        {
            this.Text = "InputLocker";
            this.inputBlocker = new InputBlocker(Keys.Pause);
            this.inputBlocker.InputBlocked += onInputBlocked;
            this.inputBlocker.BlockingStateChanged += onBlockingStateChanged;
        }

        #region UI

        protected override void OnLoad(EventArgs e)
        {
            this.Visible = false;
            this.ShowInTaskbar = false;
            this.trayIcon = this.createTrayIcon();

            SystemEvents.DisplaySettingsChanged += this.onDisplaySettingsChanged;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.inputBlocker.Dispose();
        }


        /// <summary>
        /// Creates a tray icon
        /// </summary>
        private NotifyIcon createTrayIcon()
        {
            var trayIcon = new NotifyIcon()
            {
                Text = "Keyboard Locker",
                Icon = this.getIcon(),
                ContextMenu = this.createContextMenu(),
                Visible = true
            };

            trayIcon.MouseUp += onTrayIconClick;
            return trayIcon;
        }


        /// <summary>
        /// Updates the look
        /// </summary>
        private void updateLook()
        {
            this.trayIcon.Icon = this.getIcon();
        }


        /// <summary>
        /// Returns the icon from the resource
        /// </summary>
        private Icon getIcon()
        {
            var lightMode = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", true)?.GetValue("SystemUsesLightTheme") as int? == 1;
            var locked = this.inputBlocker.IsBlocking;
            var iconName = $"{this.GetType().Namespace}.Icon{(locked ? "Locked" : "Unlocked")}{(lightMode ? "Light" : "Dark")}.png";

            using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(iconName))
            using (var bmp = new Bitmap(s))
                return Icon.FromHandle(bmp.GetHicon());
        }


        /// <summary>
        /// Creates the context menu
        /// </summary>
        private ContextMenu createContextMenu()
        {
            var trayMenu = new ContextMenu();

            trayMenu.MenuItems.Add("Start with Windows", this.onStartUp).Checked = this.startsWithWindows();
            trayMenu.MenuItems.Add("About...", this.onAbout);
            trayMenu.MenuItems.Add("-");
            trayMenu.MenuItems.Add("Exit", this.onMenuExit);

            return trayMenu;
        }

        #endregion

        #region Interactions

        /// <summary>
        /// Shows the tooltip
        /// </summary>
        private void showToolTip(bool blockingState)
        {
            if (blockingState != this.inputBlocker.IsBlocking)
                return;

            this.trayIcon.Visible = true;
            if (blockingState)
                this.trayIcon.ShowBalloonTip(10000, null, $"Your keyboard and mouse is locked. Press \"{this.inputBlocker.ControlKey}\" to unlock.", ToolTipIcon.Warning);
            //else
            //    this.trayIcon.ShowBalloonTip(1000, null, "Your keyboard and mouse is unlocked.", ToolTipIcon.Info);
        }

        
        /// <summary>
        /// Plays the notification
        /// </summary>
        private void playNotificationSound(bool blocking)
        {
            try
            {
                var soundFile = $"{Environment.ExpandEnvironmentVariables("%SystemRoot%")}/Media/{(blocking ? "Speech On.wav" :"Speech Sleep.wav")}";
                using (var player = new SoundPlayer(soundFile))
                    player.Play();
            }
            catch { }
        }

        #endregion

        #region Start-up Handling

        /// <summary>
        /// Setting the startup state
        /// </summary>
        private bool startsWithWindows()
        {
            try
            {
                return Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true).GetValue(this.Text) as string == Application.ExecutablePath.ToString();
            }
            catch { return false; }
        }


        /// <summary>
        /// Setting the startup state
        /// </summary>
        private bool setStartup(bool set)
        {
            try
            {
                var rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (set)
                    rk.SetValue(this.Text, Application.ExecutablePath.ToString());
                else
                    rk.DeleteValue(this.Text, false);

                return set;
            }
            catch { return !set; }
        }

        #endregion

        #region Event Handlers

        async private void onDisplaySettingsChanged(object sender, EventArgs e)
        {
            await Task.Delay(500);
            this.updateLook();

            await Task.Delay(1000);
            this.updateLook();
        }

        private void onTrayIconClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                this.inputBlocker.StartBlocking();
        }


        private void onBlockingStateChanged(bool state)
        {
            this.updateLook();
            this.playNotificationSound(state);
            this.showToolTip(state);
        }

        private void onInputBlocked()
        {
            if ((DateTime.Now - lastKeyBlockedNotification).TotalSeconds < 5)
                return;

            lastKeyBlockedNotification = DateTime.Now;
            this.showToolTip(true);
        }

        #endregion

        #region Menu Handlers

        private void onStartUp(object sender, EventArgs e)
        {
            var btn = (sender as MenuItem);
            btn.Checked = this.setStartup(!btn.Checked);
        }

        private void onAbout(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/poulicek/BrightnessControl"));
        }


        private void onMenuExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        #endregion
    }
}
