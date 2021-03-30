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
        private const float r = 100000; // relative values resolution


        public void ProcessRequest(HttpContext context)
        {
            switch (context.Request.Query["v"])
            {
                case "screen":
                    {
                        var cutout = context.Request.Query["w"]?.Split(',');
                        context.Response.Write(this.getScreenShot(this.readRelativeCutout(cutout)), "image/png");
                        break;
                    }

                case "click":
                    {
                        if (float.TryParse(context.Request.Query["x"], out var xRatio) && float.TryParse(context.Request.Query["y"], out var yRatio))
                            this.perfromMouseClick(xRatio, yRatio, int.TryParse(context.Request.Query["b"], out var btn) ? btn : 1);
                        break;
                    }
            }
        }


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


        private Rectangle projectCutout(RectangleF relativeCutout, Rectangle screenRect)
        {
            if (relativeCutout.IsEmpty)
                return screenRect;

            var rangeW = (int)Math.Ceiling(relativeCutout.Width * screenRect.Width);
            var rangeH = (int)Math.Ceiling(relativeCutout.Height * screenRect.Height);
            var rangeX = (int)(relativeCutout.X * (screenRect.Width - rangeW));
            var rangeY = (int)(relativeCutout.Y * (screenRect.Height - rangeH));

            return new Rectangle(rangeX, rangeY, rangeW, rangeH);
        }


        private byte[] getScreenShot(RectangleF cutout)
        {
            const float inflation = 0.3f;

            var screenRect = Screen.AllScreens[0].Bounds;
            var cutoutRect = this.projectCutout(cutout, screenRect);
            cutoutRect.Inflate((int)(cutoutRect.Width * inflation), (int)(cutoutRect.Height * inflation));

            using (var img = new Bitmap(screenRect.Width, screenRect.Height, PixelFormat.Format24bppRgb))
            using (var g = Graphics.FromImage(img))
            using (var ms = new MemoryStream())
            {
                g.CopyFromScreen(cutoutRect.X, cutoutRect.Y, cutoutRect.X, cutoutRect.Y, cutoutRect.Size);
                img.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }            
        }



        private void perfromMouseClick(float xRatio, float yRatio, int btn)
        {
            var rect = Screen.AllScreens[0].Bounds;
            var x = (int)(xRatio * rect.Width);
            var y = (int)(yRatio * rect.Height);

            InputHelper.MouseClick(x, y, (InputHelper.MouseButton)btn);
        }


        private Bitmap resample(Bitmap src, int maxWidth, int maxHeight)
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
                g.DrawImage(src, new Rectangle(0, 0, w, h), 0, 0, src.Width, src.Height, GraphicsUnit.Pixel);

            return dst;
        }
    }
}
