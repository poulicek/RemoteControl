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
                    var s = this.getScreenShot();
                    context.Response.Write(s, "image/jpeg");
                    break;

                case "click":
                    if (int.TryParse(context.Request.Query["x"], out var x) && int.TryParse(context.Request.Query["y"], out var y))
                        InputHelper.LeftMouseClick(x, y);
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
    }
}
