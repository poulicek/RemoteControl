using System;
using System.Drawing;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Common;
using KeyboardLocker.Input;

namespace KeyboardLocker.UI
{
    public class TrayIcon : TrayIconBase
    {


        private readonly Bitmap iconLock;
        private readonly Bitmap iconScreen;
        private readonly SoundPlayer soundBlock;
        private readonly SoundPlayer soundUnblock;
        private readonly SoundPlayer soundLongPress;
        private readonly InputBlocker inputBlocker;


        public TrayIcon() : base("Keyboard Locker")
        {
#if DEBUG
            HooksManager.KeyPressed += this.onKeyPressed;
#else
            HooksManager.KeyBlocked += this.onKeyBlocked;
#endif

            this.inputBlocker = new InputBlocker(Keys.Pause, Keys.Pause);
            this.inputBlocker.ScreenTurnedOff += this.onScreenTurnedOff;
            this.inputBlocker.ScreenOffRequested += this.onScreenOffRequested;
            this.inputBlocker.BlockingStateChanged += this.onBlockingStateChanged;

            this.soundBlock = this.getSound("Lock.wav");
            this.soundUnblock = this.getSound("Unlock.wav");
            this.soundLongPress = this.getSound("LongPress.wav");

            this.iconLock = this.getResourceImage("Resources.IconLocked.png");
            this.iconScreen = this.getResourceImage("Resources.IconScreenOff.png");
        }

        #region UI

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            this.soundBlock.Dispose();
            this.soundUnblock.Dispose();
            this.inputBlocker.Dispose();
            this.iconLock.Dispose();
        }


        /// <summary>
        /// Returns the name of the icon
        /// </summary>
        protected override string getIconName(bool lightMode)
        {
            var locked = this.inputBlocker.IsBlocking;
            return $"Resources.Icon{(locked ? "Locked" : "Unlocked")}{(lightMode ? "Light" : "Dark")}.png";
        }


        /// <summary>
        /// Shows the tooltip
        /// </summary>
        private void showToolTip(bool blockingState)
        {
            try
            {
                if (blockingState != this.inputBlocker.IsBlocking)
                    return;

                this.trayIcon.Visible = true;
                if (blockingState)
                    BalloonTooltip.Show(
                        this.iconLock,
                        $"Your keyboard and mouse is locked.{Environment.NewLine}Press \"{this.inputBlocker.UnblockingKey}\" to unlock.",
                        $"Hold \"{this.inputBlocker.UnblockingKey}\" to turn off the screen.",
                        5000);
                else
                    BalloonTooltip.Hide();
            }
            catch { }
        }
        

        /// <summary>
        /// Returns the sound object
        /// </summary>
        private SoundPlayer getSound(string fileName)
        {
            return new SoundPlayer(this.getResourceStream($"Resources.{fileName}"));
        }

        
        /// <summary>
        /// Plays the notification
        /// </summary>
        private void playNotificationSound(bool block)
        {
            if (block)
                this.soundBlock?.Play();
            else
                this.soundUnblock?.Play();
        }

        #endregion

        #region Event Handlers

        protected override void onTrayIconClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                this.inputBlocker.StartBlocking();
        }

        private void onBlockingStateChanged(bool state)
        {
            this.updateLook();
            this.showToolTip(state);
            this.playNotificationSound(state);
        }

        private void onKeyBlocked()
        {
            this.showToolTip(true);
        }

        private void onScreenOffRequested()
        {
            BalloonTooltip.Show(this.iconScreen, "Turning the screen off...");
            //this.soundLongPress.Play();
        }

        private void onKeyPressed(Keys key)
        {
            BalloonTooltip.Show(this.iconLock, key.ToString());
        }

        private void onScreenTurnedOff()
        {
            BalloonTooltip.Hide();
        }

        #endregion
    }
}
