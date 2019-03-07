using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http
{
    public class Response : Message
    {
        public int Status { get; set; } = 200;

        public string StatusMessage = "OK";

        public void Load(byte[] bytes)
        {
            base.Load(bytes);

            var content = Encoding.ASCII.GetString(bytes);
        }

        public byte[] GetBytes()
        {
            String status = $"HTTP/1.0 {this.Status} {this.StatusMessage}\r\n";

            String headers = "";

            if (!IsClosableConnection())
            {
                headers += $"Content-length: {Body.Length}\r\n";
            }

            foreach (var keyValuePair in this.Headers)
            {
                if (!keyValuePair.Key.ToLower().Equals("content-length"))
                {
                    headers += $"{keyValuePair.Key}: {keyValuePair.Value}\r\n";
                }
            }

            var upperContent = Encoding.ASCII.GetBytes($"{status}{headers}\r\n");

            return upperContent.Concat(this.Body).ToArray();
        }
    }
}
