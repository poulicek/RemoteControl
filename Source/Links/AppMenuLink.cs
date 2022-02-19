using System;
using System.Diagnostics;

namespace RemoteControl.Links
{
    public class AppMenuLink : MenuLinkBase
    {
        private Process process;

        public AppMenuLink(string file) : base(file)
        {
        }


        public override byte[] GetIcon(out string mime)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Launches the app link
        /// </summary>
        public override void Launch()
        {
            // commented as use of Process.Start is restricted on Microsoft Store
            //if (this.process == null || this.process.HasExited)
            //    this.process = Process.Start(this.Link);
        }


        /// <summary>
        /// Reads the app link file
        /// </summary>
        protected override bool ReadFile(string file)
        {
            throw new NotImplementedException();
        }
    }
}
