using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Http
{
    public abstract class Message
    {
        public string FirstLine { get; set; }

        public string Content { get; set; } //TODO weghalen

        public Dictionary<string, string> Headers;

        public byte[] Body { get; set; }

        public Message()
        {
            Headers = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        }

        public void Load(byte[] bytes)
        {
            var content = Encoding.ASCII.GetString(bytes);
            this.Content = content;

            this.Headers = ParseHeaders(content);

            this.Body = ParseBody(bytes);
        }

        public Boolean IsComplete()
        {
            int parsed;
            return this.Content.Contains("\r\n\r\n") && (
                       !Headers.ContainsKey("Content-length") || 
                       (Int32.TryParse(Headers["Content-length"], out parsed) && parsed == Body.Length)
                       );
        }

        private Dictionary<string, string> ParseHeaders(string content)
        {
            var headers = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            string[] lines = content.Split(new[] {"\r\n"}, StringSplitOptions.None);

            for (int x = 1; x < lines.Length; x++)
            {
                string line = lines[x];

                if (line == "")
                {
                    break;
                }

                var index = line.IndexOf(":", StringComparison.Ordinal);
                if (index != -1)
                {
                    headers.Add(line.Substring(0, index), line.Substring(index + 1));
                }
            }

            return headers;
        }

        private byte[] ParseBody(byte[] content)
        {
            var str = Encoding.ASCII.GetString(content);

            var contentStart = str.IndexOf("\r\n\r\n", StringComparison.Ordinal) + 4;
            var contentLength = 0;

            int parsed;
            if (Headers.ContainsKey("content-length") && Int32.TryParse(Headers["Content-length"], out parsed))
            {
                contentLength = parsed;
            }

            if (content.Length >= contentStart + contentLength)
            {
                return content.Skip(contentStart).Take(contentLength).ToArray();
            }
            else
            {
                //not complete
                return new byte[0];
            }
        }

    }
}
