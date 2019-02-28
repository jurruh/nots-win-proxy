using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http
{
    public class Response : Message
    {
        public void Load(byte[] bytes)
        {
            base.Load(bytes);

            var content = Encoding.ASCII.GetString(bytes);
        }

        public byte[] GetBytes()
        {
            String status = $"HTTP/1.0 200 OK\r\n";

            String headers = "";

            headers += $"Content-length: {Body.Length}\r\n";
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
