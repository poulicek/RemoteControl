﻿using System;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Forms;
using Common;
using KeyboardLocker.Input;

namespace KeyboardLocker.UI
{
    public class TrayIcon : TrayIconBase
    {
        private readonly Bitmap notificationIcon;
        private readonly SoundPlayer soundBlock;
        private readonly SoundPlayer soundUnblock;
        private readonly InputBlocker inputBlocker;


        public TrayIcon() : base("Keyboard Locker")
        {
            this.inputBlocker = new InputBlocker(Keys.Pause);
            this.inputBlocker.InputBlocked += this.onInputBlocked;
            this.inputBlocker.BlockingStateChanged += this.onBlockingStateChanged;

            this.soundBlock = this.getSound(true);
            this.soundUnblock = this.getSound(false);

            this.notificationIcon = this.getResourceImage("Resources.IconLocked.png");
        }

        #region UI

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            this.soundBlock.Dispose();
            this.soundUnblock.Dispose();
            this.inputBlocker.Dispose();
            this.notificationIcon.Dispose();
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
                    NotificationHelper.Show(this.notificationIcon, $"Your keyboard and mouse is locked.{Environment.NewLine}Press \"{this.inputBlocker.ControlKey}\" to unlock.", 5000);
                else
                    NotificationHelper.Hide();
            }
            catch { }
        }
        

        /// <summary>
        /// Returns the sound object
        /// </summary>
        private SoundPlayer getSound(bool block)
        {
            return new SoundPlayer(this.getResourceStream($"Resources.{(block ? "Lock" : "Unlock")}.wav"));
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
            this.playNotificationSound(state);
            this.showToolTip(state);
        }

        private void onInputBlocked()
        {
            this.showToolTip(true);
        }

        #endregion
    }
}
