using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using RemoteControl.Server;
using TrayToolkit.Helpers;

namespace RemoteControl.Controllers.Rdp
{
    public class RdpController : IController
    {
        public event Action SessionChanged;

        private Point lastCursor;
        private string lastSession = string.Empty;

        public void ProcessRequest(HttpContext context)
        {
            switch (context.Request.Query["v"])
            {
                case "screen":
                    this.processScreenRequest(context);
                    break;

                case "stream":
                    this.writeMJPEG(context.Response, this.readScreen(context.Request.Query["e"]));
                    break;

                case "click":
                    this.processClickRequest(context);
                    break;
            }
        }


        /// <summary>
        /// Converts the hexa formatted numeric attribute to a float number
        /// </summary>
        private float denc(string str)
        {
            if (string.IsNullOrEmpty(str))
                return float.NaN;

            try
            {
                var sign = str[0] == '-' ? -1 : 1;
                var num = Convert.ToInt32(str.TrimStart('-'), 16);
                return sign * num / 100000f;
            }
            catch { return float.NaN; }
        }


        /// <summary>
        /// Processes the screen request
        /// </summary>
        private void processScreenRequest(HttpContext context)
        {
            // processing the click request provided together with the screen
            if (context.Request.Query["b"] != null)
                this.processClickRequest(context);

            var session = context.Request.Query["s"] ?? string.Empty;
            var setCursor = int.TryParse(context.Request.Query["u"], out var u) && u == 1;
            var screen = this.readScreen(context.Request.Query["e"]);
            var cutout = this.projectCutout(this.readRelativeCutout(context.Request.Query["w"]?.Split(',')), screen.Bounds);

            this.handleScreenRequest(context.Response, session, screen, cutout, setCursor);
        }


        /// <summary>
        /// Processes the click request
        /// </summary>
        private void processClickRequest(HttpContext context)
        {
            var xRatio = this.denc(context.Request.Query["x"]);
            var yRatio = this.denc(context.Request.Query["y"]);

            if (float.IsNaN(xRatio) || float.IsNaN(yRatio))
                return;

            var screen = this.readScreen(context.Request.Query["e"]);
            var btn = int.TryParse(context.Request.Query["b"], out var b) ? b : 1;

            if (btn == 0)
                this.perfromMouseMove(screen, xRatio, yRatio);
            else if (btn != (int)InputHelper.MouseButton.Middle)
                this.perfromMouseClick(screen, xRatio, yRatio, btn);
            else
            {
                var mxRatio = this.denc(context.Request.Query["mx"]);
                var myRatio = this.denc(context.Request.Query["my"]);

                if (!float.IsNaN(mxRatio) && !float.IsNaN(myRatio))
                    this.perfromWheelScroll(screen, xRatio, yRatio, mxRatio, myRatio);
            }
        }


        /// <summary>
        /// Reads the screen parameter
        /// </summary>
        private ScreenBounds readScreen(string value)
        {
            return new ScreenBounds(int.TryParse(value, out var screenIdx) ? Screen.AllScreens[screenIdx % Screen.AllScreens.Length] : Screen.PrimaryScreen);
        }


        /// <summary>
        /// Handles the screenshot request
        /// </summary>
        private void handleScreenRequest(HttpResponse r, string session, ScreenBounds screen, Rectangle cutout, bool setCursor)
        {
            // detection of new session so the user can be notified
            if (session != this.lastSession)
            {
                this.SessionChanged?.Invoke();
                this.lastSession = session;
            }

            // setting the cursor position to cutout's center
            if (setCursor)
                this.updateCursor(screen.Project(cutout.GetCenter()));

            // inflating the cutout so the panning is more smooth
            const float inflation = 0.3f;
            cutout.Inflate((int)(cutout.Width * inflation), (int)(cutout.Height * inflation));

            // setting the PNG format for smaller sizes
            var codec = this.getEncoder(2 * cutout.Width * cutout.Height > screen.Width * screen.Height ? ImageFormat.Jpeg : ImageFormat.Png);
            this.writeScreenShot(r, screen, cutout, codec);
        }


        /// <summary>
        /// Writes the screen picture as an MJPEG stream
        /// </summary>
        private void writeMJPEG(HttpResponse r, ScreenBounds screen)
        {
            this.SessionChanged?.Invoke();

            var codec = this.getEncoder(ImageFormat.Jpeg);

            r.ApplyGzipCompression = false;
            r.WriteHeader("multipart/x-mixed-replace; boundary=\"RDP_MJPEG\"");
            while (true)
            {
                r.Write($"--RDP_MJPEG\r\n");
                r.Write($"Content-Type: {codec.MimeType}\r\n\r\n");
                this.writeScreenShot(r, screen, screen.Bounds, codec);
            }
        }


        /// <summary>
        /// Writes a screenshot
        /// </summary>
        private void writeScreenShot(HttpResponse r, ScreenBounds screen, Rectangle cutout, ImageCodecInfo codec)
        {
            using (var screenImg = new Bitmap(screen.Width, screen.Height))
            using (var g = Graphics.FromImage(screenImg))
            {
                g.CopyFromScreen(cutout.X, cutout.Y, cutout.X - screen.X, cutout.Y - screen.Y, cutout.Size);                

                using (var ms = new MemoryStream())
                using (var canvasImg = this.projectCoutout(screen, screenImg, cutout, codec.FormatID == ImageFormat.Png.Guid))
                {
                    canvasImg.Save(ms, codec, this.getQualityParams(25));
                    r.Write(ms, codec.MimeType);
                }
            }
        }


        /// <summary>
        /// Projects the cutout of the screen considering the scale
        /// </summary>
        private Bitmap projectCoutout(ScreenBounds screen, Bitmap screenImg, Rectangle cutoutRect, bool highQuality)
        {
            if (screen.Scale <= 1)
                return screenImg;

            var canvasImg = new Bitmap(screen.ProjectedWidth, screen.ProjectedHeight);
            using (var canvasG = Graphics.FromImage(canvasImg))
            {
                canvasG.SetQuality(highQuality);
                canvasG.DrawImage(screenImg, screen.Project(cutoutRect), cutoutRect, GraphicsUnit.Pixel);
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

            return new RectangleF(this.denc(cutout[0]), this.denc(cutout[1]), this.denc(cutout[2]), this.denc(cutout[3]));
        }


        /// <summary>
        /// Projects the relative cutout to screen coordinates
        /// </summary>
        private Rectangle projectCutout(RectangleF relativeCutout, Rectangle screen)
        {
            if (relativeCutout.IsEmpty)
                return screen;

            var rangeW = (int)Math.Ceiling(relativeCutout.Width * screen.Width);
            var rangeH = (int)Math.Ceiling(relativeCutout.Height * screen.Height);
            var rangeX = screen.X + (int)(relativeCutout.X * (screen.Width - rangeW));
            var rangeY = screen.Y + (int)(relativeCutout.Y * (screen.Height - rangeH));

            return new Rectangle(rangeX, rangeY, rangeW, rangeH);
        }


        /// <summary>
        /// Performs a mouse move on the provided location
        /// </summary>
        private void perfromMouseMove(ScreenBounds screen, float xRatio, float yRatio)
        {
            var pt = screen.Project(new Point((int)(screen.X + xRatio * screen.Width), (int)(screen.Y + yRatio * screen.Height)));
            this.updateCursor(pt);
        }


        /// <summary>
        /// Performs a mouse click on the provided location
        /// </summary>
        private void perfromMouseClick(ScreenBounds screen, float xRatio, float yRatio, int btn)
        {
            var pt = screen.Project(new Point((int)(screen.X + xRatio * screen.Width), (int)(screen.Y + yRatio * screen.Height)));
            InputHelper.MouseAction((InputHelper.MouseButton)btn, pt, InputHelper.MouseClickMode.Click);
        }


        /// <summary>
        /// Performs a wheel scroll on the provided location
        /// </summary>
        private void perfromWheelScroll(ScreenBounds screen, float xRatio, float yRatio, float mxRatio, float myRatio)
        {
            this.perfromMouseMove(screen, mxRatio, myRatio);

            // using the fixed scroll step seems more natural
            //var scroll = screen.Project(new Point((int)(xRatio * screen.Width), (int)(yRatio * screen.Height)));
            //InputHelper.MouseScroll(scroll.X, scroll.Y);

            InputHelper.MouseScroll(Math.Sign(xRatio) * 160, Math.Sign(yRatio) * 160);
        }


        /// <summary>
        /// Updates cursor position if changed
        /// </summary>
        private void updateCursor(Point pt)
        {
            if (pt.IsEmpty)
                return;

            if (this.lastCursor != pt)
                InputHelper.SetCursorPosition(pt.X, pt.Y);
            this.lastCursor = pt;
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
