using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using RemoteControl.Server;
using TrayToolkit.Helpers;

namespace RemoteControl.Controllers
{
    public class RdpController : IController
    {
        public event Action SessionChanged;

        private const float r = 100000; // relative values resolution
        private Point lastCursor;
        private string lastSession = string.Empty;


        public void ProcessRequest(HttpContext context)
        {
            switch (context.Request.Query["v"])
            {
                case "screen":
                    {
                        var data = this.handleScreenRequest(context.Request.Query["s"] ?? string.Empty, context.Request.Query["w"]?.Split(','), out var codec);
                        context.Response.Write(data, codec.MimeType);
                        break;
                    }

                case "click":
                    {
                        if (int.TryParse(context.Request.Query["x"], out var xRatio) && int.TryParse(context.Request.Query["y"], out var yRatio))
                            this.perfromMouseClick(xRatio / r, yRatio / r, int.TryParse(context.Request.Query["b"], out var btn) ? btn : 1);
                        break;
                    }
            }
        }


        /// <summary>
        /// Handles the screenshot request
        /// </summary>
        private byte[] handleScreenRequest(string session, string[] cutout, out ImageCodecInfo codec)
        {
            // detection of new session so the user can be notified
            if (session != this.lastSession)
            {
                this.SessionChanged?.Invoke();
                this.lastSession = session;
            }

            // projecting the cutout to the screen
            var screenSize = ScreenHelper.GetScaledScreenSize();
            var cutoutRect = this.projectCutout(this.readRelativeCutout(cutout), screenSize);
            var center = this.projectPoint(cutoutRect.X + cutoutRect.Width / 2, cutoutRect.Y + cutoutRect.Height / 2, screenSize);

            // setting the cursor position to cutout's center
            if (lastCursor != center)
            {
                InputHelper.SetCursorPosition(center.X, center.Y);
                lastCursor = center;
            }

            // inflating the cutout so the panning is more smooth
            const float inflation = 0.3f;
            cutoutRect.Inflate((int)(cutoutRect.Width * inflation), (int)(cutoutRect.Height * inflation));

            return this.getScreenShot(cutoutRect, screenSize, out codec);
        }


        /// <summary>
        /// Returns a screenshot
        /// </summary>
        private byte[] getScreenShot(Rectangle cutout, Size screenSize, out ImageCodecInfo codec)
        {
            // setting the PNG format for smaller sizes
            var format = 2 * cutout.Width * cutout.Height > screenSize.Width * screenSize.Height ? ImageFormat.Jpeg : ImageFormat.Png;

            using (var screenImg = new Bitmap(screenSize.Width, screenSize.Height))
            using (var screenG = Graphics.FromImage(screenImg))
            using (var ms = new MemoryStream())
            {
                screenG.CopyFromScreen(cutout.X, cutout.Y, cutout.X, cutout.Y, cutout.Size);
                using (var canvasImg = this.projectCoutout(screenImg, screenSize, cutout, format == ImageFormat.Png))
                    canvasImg.Save(ms, codec = this.getEncoder(format), this.getQualityParams(25));

                return ms.ToArray();
            }
        }


        /// <summary>
        /// Projects the point to screen coordinates
        /// </summary>
        private Point projectPoint(float x, float y, Size screenSize)
        {
            var scale = this.getScreenScale(screenSize);
            return new Point((int)(x / scale), (int)(y / scale));
        }


        /// <summary>
        /// Returns the scale of the screen
        /// </summary>
        private float getScreenScale(Size screenSize)
        {
            return (float)screenSize.Width / Screen.PrimaryScreen.Bounds.Width;
        }


        /// <summary>
        /// Projects the cutout of the screen considering the scale
        /// </summary>
        private Bitmap projectCoutout(Bitmap screenImg, Size screenSize, Rectangle cutoutRect, bool highQuality)
        {
            var scale = this.getScreenScale(screenSize);
            if (scale <= 1)
                return screenImg;

            var canvasSize = new Size((int)Math.Ceiling(screenSize.Width / scale), (int)Math.Ceiling(screenSize.Height / scale));
            var canvasImg = new Bitmap(canvasSize.Width, canvasSize.Height);
            using (var canvasG = Graphics.FromImage(canvasImg))
            {
                canvasG.SetQuality(highQuality);
                canvasG.DrawImage(screenImg, new RectangleF(cutoutRect.X / scale, cutoutRect.Y / scale, cutoutRect.Width / scale, cutoutRect.Height / scale), cutoutRect, GraphicsUnit.Pixel);
            }

            return canvasImg;
        }


        /// <summary>
        /// Parses the cutout and returns its relative values (0 .. 1)
        /// </summary>
        private RectangleF readRelativeCutout(string[] cutout)
        {
            if (cutout?.Length != 4)
                return RectangleF.Empty;

            return new RectangleF(
                int.TryParse(cutout[0], out var x) ? x / r : 0,
                int.TryParse(cutout[1], out var y) ? y / r : 0,
                int.TryParse(cutout[2], out var w) ? w / r : 0,
                int.TryParse(cutout[3], out var h) ? h / r : 0);
        }


        /// <summary>
        /// Projects the relative cutout to screen coordinates
        /// </summary>
        private Rectangle projectCutout(RectangleF relativeCutout, Size screenSize)
        {
            if (relativeCutout.IsEmpty)
                return new Rectangle(Point.Empty, screenSize);

            var rangeW = (int)Math.Ceiling(relativeCutout.Width * screenSize.Width);
            var rangeH = (int)Math.Ceiling(relativeCutout.Height * screenSize.Height);
            var rangeX = (int)(relativeCutout.X * (screenSize.Width - rangeW));
            var rangeY = (int)(relativeCutout.Y * (screenSize.Height - rangeH));

            return new Rectangle(rangeX, rangeY, rangeW, rangeH);
        }


        /// <summary>
        /// Performs a mouse click on the provided location
        /// </summary>
        private void perfromMouseClick(float xRatio, float yRatio, int btn)
        {
            var s = ScreenHelper.GetScaledScreenSize();
            var pt = this.projectPoint(xRatio * s.Width, yRatio * s.Height, s);;

            if (btn == (int)InputHelper.MouseButton.Middle)
                InputHelper.MouseScroll(pt.X, pt.Y);
            else
                InputHelper.MouseClick(pt.X, pt.Y, (InputHelper.MouseButton)btn);
        }


        /// <summary>
        /// Returns the quality parameters for the JPEG encoder
        /// </summary>
        private EncoderParameters getQualityParams(long quality)
        {
            var encParams = new EncoderParameters(1);
            encParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);

            return encParams;
        }


        /// <summary>
        /// Returns the encoder based on the image format
        /// </summary>
        private ImageCodecInfo getEncoder(ImageFormat format)
        {
            foreach (var codec in ImageCodecInfo.GetImageEncoders())
                if (codec.FormatID == format.Guid)
                    return codec;

            return null;
        }
    }
}
