using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using RemoteControl.UI;
using TrayToolkit.Helpers;

namespace RemoteControl.Controllers.Grip
{
    public class GripButtonMouse : IGripButton
    {
        private const int FRAME_DELAY = 20;
        private const int MAX_INPUT_DELAY = 5;
        private const int MAX_CLICK_DELAY = 500;
        private const float SPEED = 10;
        private const float ACCELERATION = 2f;        

        private readonly Timer timer;

        private bool isMoving;
        private bool hasMoved;
        private PointF lastCoords;

        private DateTime lastPressTime;
        private DateTime lastMoveTime;

        public GripButtonMouse()
        {
             this.timer = new Timer((o) => this.move(), null, Timeout.Infinite, Timeout.Infinite);
        }


        /// <summary>
        /// Releases the button
        /// </summary>
        public void Release()
        {
            lock (this)
            {
                this.stopMoving();
                this.tryPerformClick();
            }
        }


        /// <summary>
        /// Presses the keys according to the coordinates
        /// </summary>
        public void Press(PointF coords)
        {
            this.lastCoords = coords;
            this.lastMoveTime = DateTime.Now;

            lock (this)
                this.startMoving();
        }


        /// <summary>
        /// Peforming the click with the grip button
        /// </summary>
        private void tryPerformClick()
        {
            // returns if the mouse has moved or the button was pressed too long
            if (this.hasMoved || (DateTime.Now - this.lastPressTime).TotalMilliseconds > MAX_CLICK_DELAY)
                return;

            InputHelper.MouseClick(InputHelper.MouseButton.Left);
        }


        /// <summary>
        /// Starts the cursor movement
        /// </summary>
        private void startMoving()
        {
            if (this.isMoving)
                return;

            this.isMoving = true;
            this.hasMoved = false;
            this.lastPressTime = DateTime.Now;
            this.timer.Change(0, FRAME_DELAY);
        }


        /// <summary>
        /// Stops the cursor movement
        /// </summary>
        private void stopMoving()
        {
            this.isMoving = false;
            this.lastCoords = PointF.Empty;
            this.timer?.Change(Timeout.Infinite, Timeout.Infinite);
            Pointer.HidePointer();
        }


        /// <summary>
        /// Moves the cursosr as per last known coords
        /// </summary>
        private void move()
        {
            if (!this.isMoving)
                return;

            // aborting the movement if no new coords came in
            if ((DateTime.Now - this.lastMoveTime).TotalSeconds > MAX_INPUT_DELAY)
            {
                this.stopMoving();
                return;
            }

            

            var pt = InputHelper.GetCursorPosition();
            var shiftX = Math.Sign(this.lastCoords.X) * (int)Math.Floor(Math.Pow(Math.Abs(this.lastCoords.X) * SPEED, ACCELERATION));
            var shiftY = Math.Sign(this.lastCoords.Y) * (int)Math.Floor(Math.Pow(Math.Abs(this.lastCoords.Y) * SPEED, ACCELERATION));

            // returns if no movement is visible
            if (shiftX != 0 || shiftY != 0)
            {

                pt.Offset(shiftX, -shiftY);
                InputHelper.SetCursorPosition(pt.X, pt.Y);
                this.hasMoved = true;
            }

            Pointer.ShowPointer(InputHelper.GetCursorPosition());
        }
    }
}
