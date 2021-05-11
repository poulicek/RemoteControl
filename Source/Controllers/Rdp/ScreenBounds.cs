using System.Drawing;
using System.Windows.Forms;
using TrayToolkit.Helpers;

namespace RemoteControl.Controllers.Rdp
{
    public class ScreenBounds
    {
        public int ProjectedWidth { get; }
        public int ProjectedHeight { get; }

        public int X { get { return this.Bounds.X; } }
        public int Y { get { return this.Bounds.Y; } }
        public int Width { get { return this.Bounds.Width; } }
        public int Height { get { return this.Bounds.Height; } }

        public float Scale { get; }
        public Rectangle Bounds { get; }


        public ScreenBounds(Screen screen)
        {
            this.Bounds = screen.GetScaledBounds();
            this.Scale = (float)this.Bounds.Width / screen.Bounds.Width;
            this.ProjectedWidth = screen.Bounds.Width;
            this.ProjectedHeight = screen.Bounds.Height;
        }


        /// <summary>
        /// Projects the to screen coordinates
        /// </summary>
        public Point Project(Point pt)
        {
            return new Point((int)(pt.X / this.Scale), (int)(pt.Y / this.Scale));
        }


        /// <summary>
        /// Projects the to screen bounds
        /// </summary>
        public RectangleF Project(Rectangle r)
        {
            return new RectangleF(r.X / this.Scale, r.Y / this.Scale, r.Width / this.Scale, r.Height / this.Scale);
        }
    }
}
