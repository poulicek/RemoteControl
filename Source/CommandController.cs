using System;
using System.IO;
using System.Reflection;
using RemoteControl.Server;

namespace RemoteControl
{
    class CommandController : IDisposable
    {
        private readonly HttpServer server = new HttpServer();

        public CommandController()
        {
            this.server.CommandReceived += this.onCommandReceived;
            this.server.Listen(50000);
        }

        private void onCommandReceived(string command, string value)
        {
            switch (command)
            {


                default:
                    this.server.WriteText(this.getTextResource("index.html"));
                    break;
            }
        }

        private string getTextResource(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var localFile = Path.Combine("../../Resources", fileName);
            if (File.Exists(localFile))
                return File.ReadAllText(localFile);

            using (var s = assembly.GetManifestResourceStream("RemoteControl.Resources." + fileName))
            using (var r = new StreamReader(s))
                return r.ReadToEnd();
        }

        public void Dispose()
        {
            this.server.Dispose();
        }
    }
}
