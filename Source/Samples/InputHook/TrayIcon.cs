using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Win32;

namespace InputHookWin
{
    public class TrayIcon : Form
    {
        private NotifyIcon trayIcon;
        private readonly InputBlocker inputBlocker;


        public TrayIcon()
        {
            this.inputBlocker = new InputBlocker(new KeyCombination(Keys.Pause));
        }


        /// <summary>
        /// Returns the icon from the resource
        /// </summary>
        private Icon getIcon()
        {
            var lightMode = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", true)?.GetValue("SystemUsesLightTheme") as int? == 1;

            using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(lightMode ? "InputHookWin.IconLight.png" : "InputHookWin.IconDark.png"))
            using (var bmp = new Bitmap(s))
                return Icon.FromHandle(bmp.GetHicon());
        }


        protected override void OnLoad(EventArgs e)
        {
            this.hideMainWindow();
            this.trayIcon = this.createTrayIcon();
        }

        /// <summary>
        /// Hides the main window
        /// </summary>
        private void hideMainWindow()
        {
            this.Visible = false;
            this.ShowInTaskbar = false;
        }


        /// <summary>
        /// Creates a tray icon
        /// </summary>
        private NotifyIcon createTrayIcon()
        {
            var trayIcon = new NotifyIcon()
            {
                Text = "Brightness Control",
                Icon = this.getIcon(),
                ContextMenu = this.createContextMenu(),
                Visible = true
            };

            trayIcon.Click += onTrayIconClick;
            return trayIcon;
        }


        private void onTrayIconClick(object sender, EventArgs e)
        {
            this.inputBlocker.StartBlocking();
        }


        /// <summary>
        /// Creates the context menu
        /// </summary>
        private ContextMenu createContextMenu()
        {
            var trayMenu = new ContextMenu();


            //trayMenu.MenuItems.Add("-");
            //trayMenu.MenuItems.Add("Turn off screen", onTurnOff);
            //trayMenu.MenuItems.Add("-");
            //trayMenu.MenuItems.Add("Start with Windows", onStartUp).Checked = this.startsWithWindows();
            //trayMenu.MenuItems.Add("About...", onAbout);
            trayMenu.MenuItems.Add("Exit", this.onMenuExit);

            return trayMenu;
        }


        private void onMenuExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.inputBlocker.StopBlocking();
        }
    }
}
