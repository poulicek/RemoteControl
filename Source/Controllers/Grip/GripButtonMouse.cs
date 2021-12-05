using System;
using System.Drawing;
using System.Threading;
using RemoteControl.UI;
using TrayToolkit.Helpers;

namespace RemoteControl.Controllers.Grip
{
    public class GripButtonMouse : IGripButton
    {
        private const int FRAME_DELAY = 16;
        private const float SPEED = 10;
        private const float ACCELERATION = 2f;
        private const int MAX_INPUT_DELAY = 5;

        private readonly Timer timer;

        private bool isMoving;
        private PointF lastCoords;
        private DateTime coordsTime;

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
                this.stopMoving();
        }


        /// <summary>
        /// Presses the keys according to the coordinates
        /// </summary>
        public void Press(PointF coords)
        {
            this.lastCoords = coords;
            this.coordsTime = DateTime.Now;

            lock (this)
                this.startMoving();
        }


        /// <summary>
        /// Starts the cursor movement
        /// </summary>
        private void startMoving()
        {
            if (this.isMoving)
                return;

            this.isMoving = true;
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
            if ((DateTime.Now - this.coordsTime).TotalSeconds > MAX_INPUT_DELAY)
            {
                this.stopMoving();
                return;
            }

            var pt = InputHelper.GetCursorPosition();
            var shiftX = Math.Sign(this.lastCoords.X) * (int)Math.Pow(Math.Abs(this.lastCoords.X) * SPEED, ACCELERATION);
            var shiftY = Math.Sign(this.lastCoords.Y) * (int)Math.Pow(Math.Abs(this.lastCoords.Y) * SPEED, ACCELERATION);

            pt.Offset(shiftX, -shiftY);
            InputHelper.SetCursorPosition(pt.X, pt.Y);
            Pointer.ShowPointer(InputHelper.GetCursorPosition());
        }
    }
}
