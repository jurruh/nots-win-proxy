using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http
{
    public class Request : Message
    {
        public string Location { get; set; } = "/";

        public string Protocol = "HTTP/1.0";

        public string Method { get; set; } = "GET";

        public Uri Uri { get; set; }

        public override void Load(byte[] bytes)
        {
            base.Load(bytes);

            this.Method = this.ParseMethod(this.HeadersContent);
            this.Uri = this.ParseUri(this.HeadersContent);
        }


        private string ParseMethod(string content)
        {
            return content.Split(' ')[0];
        }

        public Uri ParseUri(string content)
        {
            var splitted = content.Split(' ');

            Uri outUri;

            if (splitted.Length > 1 && Uri.TryCreate(splitted[1], UriKind.Absolute, out outUri))
            {
                return outUri;
            }

            return null;
        }

        public override string ToString()
        {
            String status = $"{Method} {Uri.PathAndQuery} {Protocol}\r\n";

            String headers = "";

            foreach (var keyValuePair in Headers)
            {
                headers += $"{keyValuePair.Key}: {keyValuePair.Value}\r\n";
            }

            return $"{status}{headers}\r\n";
        }

        public int ResolveToPort()
        {
            return 80;
        }
    }
}
