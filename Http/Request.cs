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

        public string Protocol = "HTTP/1.1";

        public string Method { get; set; } = "GET";

        public void Load(byte[] bytes)
        {
            base.Load(bytes);
            var content = Encoding.ASCII.GetString(bytes);

            this.Method = this.ParseMethod(content);
            this.Location = this.ParseLocation(content);
            this.Protocol = this.ParseProtocol(content);
        }

        private string ParseProtocol(string content)
        {
            return content.Split(' ')[2];
        }

        private string ParseLocation(string content)
        {
            return content.Split(' ')[1];
        }

        private string ParseMethod(string content)
        {
            return content.Split(' ')[0];
        }

        public String ResolveToAddress()
        {
            var splitted = Content.Split(' ');

            if (splitted.Length > 1)
            {
                return splitted[1].Replace("http://", "").Replace("/", "");
            }

            return "";
        }

        public override string ToString()
        {
            String status = $"{Method} {Location} {Protocol}\r\n";

            String headers = "";

            foreach (var keyValuePair in Headers)
            {
                headers += $"{keyValuePair.Key}: {keyValuePair.Value}\r\n";
            }

            return $"{status}{headers}\r\n";
        }
    }
}
