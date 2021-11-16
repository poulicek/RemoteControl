using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace RemoteControl.Controllers.Files
{
    public class FileTemplate
    {
        #region TemplateKey Class

        private class TemplateKey
        {
            private readonly int keyLength;
            public int StartIndex { get; }
            public int EndIndex { get { return this.StartIndex + this.keyLength; } }
            
            public string Key { get; }
            public string Value { get; set; }

            public TemplateKey(int index, string key)
            {
                this.keyLength = key.Length;
                this.StartIndex = index;
                this.Key = key.Trim('{', '}');
            }
        }

        #endregion

        private readonly string template;
        private readonly List<TemplateKey> keys = new List<TemplateKey>();

        public string this[string key]
        {
            set
            {
                foreach (var tk in this.keys)
                    if (tk.Key == key)
                        tk.Value = value;
            }
        }


        public FileTemplate(string template)
        {
            this.template = template;
            this.findKeys();
        }


        private void findKeys()
        {
            this.keys.Clear();
            foreach (Match m in Regex.Matches(this.template, @"{(\w+)}", RegexOptions.Compiled))
            {
                if (m.Groups.Count > 0)
                {
                    var g = m.Groups[0];
                    this.keys.Add(new TemplateKey(g.Index, g.Value));
                }
            }
        }


        public override string ToString()
        {
            if (this.keys.Count == 0)
                return this.template;

            var index = 0;
            var sb = new StringBuilder();
            
            foreach (var tk in this.keys)
            {
                sb.Append(this.template.Substring(index, tk.StartIndex - index)).Append(tk.Value);
                index = tk.EndIndex;
            }

            sb.Append(this.template.Substring(index, this.template.Length - index));
            return sb.ToString();
        }
    }
}
