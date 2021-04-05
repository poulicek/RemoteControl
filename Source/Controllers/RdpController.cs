using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using RemoteControl.Server;
using TrayToolkit.Helpers;

namespace RemoteControl.Controllers
{
    public class RdpController : IController
    {
        private const float r = 100000; // relative values resolution


        public void ProcessRequest(HttpContext context)
        {
            switch (context.Request.Query["v"])
            {
                case "screen":
                    {
                        var cutout = context.Request.Query["w"]?.Split(',');
                        var data = this.getScreenShot(this.readRelativeCutout(cutout), out var format);
                        context.Response.Write(data, format == ImageFormat.Png ? "image/png" : "image/jpeg");
                        break;
                    }

                case "click":
                    {
                        if (float.TryParse(context.Request.Query["x"], NumberStyles.Any, CultureInfo.InvariantCulture, out var xRatio) && float.TryParse(context.Request.Query["y"], NumberStyles.Any, CultureInfo.InvariantCulture, out var yRatio))
                            this.perfromMouseClick(xRatio, yRatio, int.TryParse(context.Request.Query["b"], out var btn) ? btn : 1);
                        break;
                    }
            }
        }


        /// <summary>
        /// Returns a screenshot
        /// </summary>
        private byte[] getScreenShot(RectangleF cutout, out ImageFormat format)
        {
            const float inflation = 0.3f;

            var screenSize = ScreenHelper.GetScaledScreenSize();
            var cutoutRect = this.projectCutout(cutout, screenSize);
            cutoutRect.Inflate((int)(cutoutRect.Width * inflation), (int)(cutoutRect.Height * inflation));

            // setting the PNG format for smaller sizes
            format = 2 * cutoutRect.Width * cutoutRect.Height > screenSize.Width * screenSize.Height ? ImageFormat.Jpeg : ImageFormat.Png;

            using (var screenImg = new Bitmap(screenSize.Width, screenSize.Height, PixelFormat.Format24bppRgb))
            using (var screenG = Graphics.FromImage(screenImg))
            using (var ms = new MemoryStream())
            {
                screenG.CopyFromScreen(cutoutRect.X, cutoutRect.Y, cutoutRect.X, cutoutRect.Y, cutoutRect.Size);
                using (var canvasImg = this.projectCoutout(screenImg, screenSize, cutoutRect, format == ImageFormat.Png))
                    canvasImg.Save(ms, this.getEncoder(format), this.getQualityParams(25));

                return ms.ToArray();
            }
        }


        /// <summary>
        /// Projects the cutout of the screen considering the scale
        /// </summary>
        private Bitmap projectCoutout(Bitmap screenImg, Size screenSize, Rectangle cutoutRect, bool highQuality)
        {
            var scale = (float)screenSize.Width / Screen.AllScreens[0].Bounds.Width;
            if (scale <= 1)
                return screenImg;

            var canvasSize = new Size((int)Math.Ceiling(screenSize.Width / scale), (int)Math.Ceiling(screenSize.Height / scale));
            var canvasImg = new Bitmap(canvasSize.Width, canvasSize.Height, PixelFormat.Format24bppRgb);
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
            var x = (int)(xRatio * s.Width);
            var y = (int)(yRatio * s.Height);

            InputHelper.MouseClick(x, y, (InputHelper.MouseButton)btn);
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


        /// <summary>
        /// Resamples the image if its size exceeds on of the maximums
        /// </summary>
        /// <returns></returns>
        private Bitmap resample(Bitmap src, int maxWidth, int maxHeight, bool highQuality)
        {
            if (src.Width <= maxWidth && src.Height <= maxHeight)
                return src;

            var w = src.Width;
            var h = src.Height;

            if (w > maxWidth)
            {
                w = maxWidth;
                h = src.Height * maxWidth / src.Width;
            }

            if (h > maxHeight)
            {
                w = src.Width * maxHeight / src.Height;
                h = maxHeight;
            }

            var dst = new Bitmap(w, h, PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(dst))
            {
                if (highQuality)
                    g.SetHighQuality();
                g.DrawImage(src, new Rectangle(0, 0, w, h), 0, 0, src.Width, src.Height, GraphicsUnit.Pixel);
            }

            return dst;
        }
    }
}
