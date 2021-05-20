namespace RemoteControl.Controllers.Menu
{
    public abstract class MenuLinkBase
    {
        public bool IsEmpty { get; } = true;

        public string Name { get; protected set; }
        
        public string Link { get; protected set; }


        protected MenuLinkBase()
        {
        }

        protected MenuLinkBase(string file)
        {
            try
            {
                this.IsEmpty = !this.ReadFile(file);
            }
            catch { }
        }

        protected MenuLinkBase(string name, string link)
        {
            this.Name = name;
            this.Link = link;
        }

        public abstract byte[] GetIcon(out string mime);

        public abstract void Launch();

        protected abstract bool ReadFile(string file);
    }
}
