using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using QRCoder;
using RemoteControl.Server;

namespace RemoteControl.Controllers
{
    public class AppController : IController
    {
        private readonly string appVersion;
        private readonly Func<string> serverUrlCallback;

        public AppController(string appVersion, Func<string> serverUrlCallback)
        {
            this.appVersion = appVersion;
            this.serverUrlCallback = serverUrlCallback;
        }

        public void ProcessRequest(HttpContext context)
        {
            switch (context.Request.Query["v"])
            {
                case "getversion":
                    var debug = string.Empty;
#if DEBUG
                    debug += ",debug";
#endif
                    context.Response.Write($"{this.appVersion},{Environment.MachineName}{debug}");
                    break;

                case "qr":
                    context.Response.Write(this.getQRCode(this.serverUrlCallback?.Invoke()), System.Web.MimeMapping.GetMimeMapping(".png"));
                    break;
            }
        }


        /// <summary>
        /// Returns the QR code stream
        /// </summary>
        private byte[] getQRCode(string str)
        {
            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(str, QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new QRCode(qrCodeData))
            using (var bmp = qrCode.GetGraphic(48, Color.White, Color.Transparent, true))
            using (var ms = new MemoryStream())
            {
                
                bmp.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }
    }
}
