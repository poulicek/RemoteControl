using System;
using System.Drawing;
using System.Threading;
using RemoteControl.Controllers.Grip;
using RemoteControl.UI;
using TrayToolkit.Helpers;

namespace RemoteControl.Controllers.Mouse
{
    public class GripButtonMouse : IGripButton
    {
        private const int FRAME_DELAY = 10;
        private const int MAX_INPUT_DELAY = 5;
        private const int MAX_CLICK_DELAY = 500;
        private const float GRIP_ZONE = 0.8f;
        private const float SPEED_GRIP = 10;
        private const float SPEED_LINEAR = 1.25f;
        private const float ACCELERATION = 2f;        

        private readonly Timer timer;

        private bool isMoving;
        private bool hasMoved;
        private PointF latestCoords;
        private PointF previousCoords;

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
            this.latestCoords = coords;
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
            this.latestCoords = PointF.Empty;
            this.previousCoords = PointF.Empty;
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
            var shift = this.getPointerShift(pt, this.latestCoords, GRIP_ZONE);

            // returns if no movement is visible
            if (!shift.IsEmpty)
            {
                pt.Offset(shift.Width, -shift.Height);
                InputHelper.SetCursorPosition(pt.X, pt.Y);
                this.hasMoved = true;
            }

            Pointer.ShowPointer(InputHelper.GetCursorPosition());
        }


        /// <summary>
        /// Returns the shift of the pointer
        /// </summary>
        private Size getPointerShift(Point cursor, PointF currentCoords, float gripZone)
        {
            if (currentCoords.IsEmpty)
            {
                this.previousCoords = PointF.Empty;
                return Size.Empty;
            }

            // computing the difference from the previous position
            var coords = new PointF(currentCoords.X - this.previousCoords.X, currentCoords.Y - this.previousCoords.Y);
            if (coords.IsEmpty)
                return Size.Empty;

            // cropping the rememberd coords to keep the cursor moving in the extreme positions
            this.previousCoords = new PointF(
                Math.Sign(currentCoords.X) * Math.Min(gripZone, Math.Abs(currentCoords.X)),
                Math.Sign(currentCoords.Y) * Math.Min(gripZone, Math.Abs(currentCoords.Y)));

            var gripMode = Math.Abs(currentCoords.X) > gripZone && Math.Abs(currentCoords.Y) > gripZone;
            return this.calculateShift(cursor, coords, gripMode);
        }


        /// <summary>
        /// Caluclates the shift of the cursos
        /// </summary>
        private Size calculateShift(Point cursor, PointF coords, bool gripMode)
        {
            if (gripMode)
            {
                return new Size(
                    Math.Sign(coords.X) * (int)Math.Floor(Math.Pow(Math.Abs(coords.X) * SPEED_GRIP, ACCELERATION)),
                    Math.Sign(coords.Y) * (int)Math.Floor(Math.Pow(Math.Abs(coords.Y) * SPEED_GRIP, ACCELERATION)));
            }

            var bounds = System.Windows.Forms.Screen.GetBounds(cursor);
            return new Size(
                Math.Sign(coords.X) * (int)Math.Ceiling(Math.Pow(Math.Abs(coords.X), SPEED_LINEAR) * bounds.Width),
                Math.Sign(coords.Y) * (int)Math.Ceiling(Math.Pow(Math.Abs(coords.Y), SPEED_LINEAR) * bounds.Height));
        }
    }
}
