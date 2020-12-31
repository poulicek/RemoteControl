using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using RemoteControl.Server;
using TrayToolkit.Helpers;
using TrayToolkit.UI;

namespace RemoteControl.Controllers
{
    public class RdpController : IController
    {
        public void ProcessRequest(HttpContext context)
        {
            switch (context.Request.Query["v"])
            {
                case "screen":
                    context.Response.Write(this.getScreenShot(), "image/jpeg");
                    break;

                case "click":
                    if (float.TryParse(context.Request.Query["x"], out var xRatio) && float.TryParse(context.Request.Query["y"], out var yRatio))
                        this.perfromMouseClick(xRatio, yRatio, int.TryParse(context.Request.Query["b"], out var btn) ? btn : 1);
                    break;
            }
        }

        private byte[] getScreenShot()
        {            
            var rect = Screen.AllScreens[0].Bounds;

            using (var bmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            using (var ms = new MemoryStream())
            {
                g.CopyFromScreen(rect.Left, rect.Top, 0, 0, rect.Size);
                bmp.Save(ms, getEncoder(ImageFormat.Jpeg), this.getQualityParams(90));
                return ms.ToArray();
            }            
        }


        private EncoderParameters getQualityParams(long quality)
        {
            var encParams = new EncoderParameters(1);
            encParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);

            return encParams;
        }


        private ImageCodecInfo getEncoder(ImageFormat format)
        {
            foreach (var codec in ImageCodecInfo.GetImageEncoders())
                if (codec.FormatID == format.Guid)
                    return codec;

            return null;
        }


        private void perfromMouseClick(float xRatio, float yRatio, int btn)
        {
            var rect = Screen.AllScreens[0].Bounds;
            var x = (int)(xRatio * rect.Width);
            var y = (int)(yRatio * rect.Height);

            InputHelper.MouseClick(x, y, (InputHelper.MouseButton)btn);
        }
    }
}
