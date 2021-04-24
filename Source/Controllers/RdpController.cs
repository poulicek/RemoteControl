﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using RemoteControl.Model;
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
                        var session = context.Request.Query["s"] ?? string.Empty;                        
                        var setCursor = int.TryParse(context.Request.Query["u"], out var u) && u == 1;
                        var screen = this.readScreen(context.Request.Query["e"]);
                        var cutout = this.projectCutout(this.readRelativeCutout(context.Request.Query["w"]?.Split(',')), screen.Bounds);

                        var data = this.handleScreenRequest(session, screen, cutout, setCursor, out var codec);
                        context.Response.Write(data, codec.MimeType);
                        break;
                    }

                case "click":
                    {
                        var screen = this.readScreen(context.Request.Query["e"]);
                        var btn = int.TryParse(context.Request.Query["b"], out var b) ? b : 1;

                        if (int.TryParse(context.Request.Query["x"], out var xRatio) && int.TryParse(context.Request.Query["y"], out var yRatio))
                        {
                            if (btn != (int)InputHelper.MouseButton.Middle)
                                this.perfromMouseClick(screen, xRatio / r, yRatio / r, btn);
                            else if (int.TryParse(context.Request.Query["mx"], out var mxRatio) && int.TryParse(context.Request.Query["my"], out var myRatio))
                                this.perfromWheelScroll(screen, xRatio / r, yRatio / r, mxRatio / r, myRatio / r);
                        }
                        break;
                    }
            }
        }


        /// <summary>
        /// Reads the screen parameter
        /// </summary>
        private ScreenModel readScreen(string value)
        {
            return new ScreenModel(int.TryParse(value, out var screenIdx) ? Screen.AllScreens[screenIdx % Screen.AllScreens.Length] : Screen.PrimaryScreen);
        }


        /// <summary>
        /// Handles the screenshot request
        /// </summary>
        private byte[] handleScreenRequest(string session, ScreenModel screen, Rectangle cutout, bool setCursor, out ImageCodecInfo codec)
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

            return this.getScreenShot(screen, cutout, out codec);
        }


        /// <summary>
        /// Returns a screenshot
        /// </summary>
        private byte[] getScreenShot(ScreenModel screen, Rectangle cutout, out ImageCodecInfo codec)
        {
            // setting the PNG format for smaller sizes
            var format = 2 * cutout.Width * cutout.Height > screen.Width * screen.Height ? ImageFormat.Jpeg : ImageFormat.Png;

            using (var screenImg = new Bitmap(screen.Width, screen.Height))
            using (var screenG = Graphics.FromImage(screenImg))
            using (var ms = new MemoryStream())
            {
                screenG.CopyFromScreen(cutout.X, cutout.Y, cutout.X - screen.X, cutout.Y - screen.Y, cutout.Size);
                using (var canvasImg = this.projectCoutout(screen, screenImg, cutout, format == ImageFormat.Png))
                    canvasImg.Save(ms, codec = this.getEncoder(format), this.getQualityParams(25));

                return ms.ToArray();
            }
        }



        /// <summary>
        /// Projects the cutout of the screen considering the scale
        /// </summary>
        private Bitmap projectCoutout(ScreenModel screen, Bitmap screenImg, Rectangle cutoutRect, bool highQuality)
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

            return new RectangleF(
                int.TryParse(cutout[0], out var x) ? x / r : 0,
                int.TryParse(cutout[1], out var y) ? y / r : 0,
                int.TryParse(cutout[2], out var w) ? w / r : 0,
                int.TryParse(cutout[3], out var h) ? h / r : 0);
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
        /// Performs a mouse click on the provided location
        /// </summary>
        private void perfromMouseClick(ScreenModel screen, float xRatio, float yRatio, int btn)
        {
            var pt = screen.Project(new Point((int)(screen.X + xRatio * screen.Width), (int)(screen.Y + yRatio * screen.Height)));
            InputHelper.MouseClick(pt.X, pt.Y, (InputHelper.MouseButton)btn);
        }


        /// <summary>
        /// Performs a wheel scroll on the provided location
        /// </summary>
        private void perfromWheelScroll(ScreenModel screen, float xRatio, float yRatio, float mxRatio, float myRatio)
        {
            var pt = screen.Project(new Point((int)(screen.X + mxRatio * screen.Width), (int)(screen.Y + myRatio * screen.Height)));
            this.updateCursor(pt);

            var scroll = screen.Project(new Point((int)(xRatio * screen.Width), (int)(yRatio * screen.Height)));
            InputHelper.MouseScroll(scroll.X, scroll.Y);
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
