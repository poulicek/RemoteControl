using System;
using System.Collections.Generic;
using System.IO;

namespace RemoteControl.Links
{
    public class MenuLinkCollection
    {
        private readonly string path;
        private readonly List<MenuLinkBase> links = new List<MenuLinkBase>();


        public MenuLinkCollection(string path)
        {
            this.path = path;
        }


        public void Load()
        {
            this.links.Clear();
            this.loadLinks<AppMenuLink>("*.lnk");
            this.loadLinks<WebMenuLink>("*.url");
        }


        private void loadLinks<T>(string searchPattern)
            where T: MenuLinkBase
        {
            foreach (var file in Directory.GetFiles(this.path, searchPattern))
            {
                var link = Activator.CreateInstance(typeof(T), file) as T;
                if (link?.IsEmpty == false)
                    this.links.Add(link);
            }
        }
    }
}
