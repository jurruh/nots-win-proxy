using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Http
{
    public class Request : Message
    {
        public Response AbortResponse { get; set; }

        public string Location { get; set; } = "/";

        public string Protocol = "HTTP/1.0";

        public string Method { get; set; } = "GET";

        public String PathAndQuery { get; set; } = "/";

        public TcpClient Sender { get; set; }

        public override void Load(byte[] bytes)
        {
            base.Load(bytes);

            this.Method = this.ParseMethod(this.HeadersContent);
            this.PathAndQuery = this.ParseUri(this.HeadersContent);
        }

        private string ParseMethod(string content)
        {
            return content.Split(' ')[0];
        }

        public String ParseUri(string content)
        {
            return content.Split(' ')[1];
        }

        public override string ToString()
        {
            String status = $"{Method} {PathAndQuery} {Protocol}\r\n";

            String headers = "";

            foreach (var keyValuePair in Headers)
            {
                headers += $"{keyValuePair.Key}: {keyValuePair.Value}\r\n";
            }

            return $"{status}{headers}\r\n";
        }
    }
}
