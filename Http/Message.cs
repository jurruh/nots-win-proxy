using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
namespace Http
{
    public abstract class Message
    {
        public string FirstLine { get; set; }

        protected string HeadersContent { get; set; } = "";

        public Dictionary<string, string> Headers { get; set; }

        public BufferBlock<byte[]> BodyStream { get; set; }

        private int CurrentBodySize { get; set; } = 0;

        public Message()
        {
            BodyStream = new BufferBlock<byte[]>();
            Headers = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        }

        public bool streamBody = false;
        public List<byte> ByteContent = new List<byte>();

        public virtual void Load(byte[] bytes)
        {
            var content = Encoding.ASCII.GetString(bytes);

            if (HasHeaders())
            {
                Stream(bytes);
            }
            else
            {
                this.HeadersContent += content;
                this.ByteContent = this.ByteContent.Concat(bytes).ToList();
                this.Headers = ParseHeaders(this.HeadersContent);

                if (HasHeaders()) {
                    var dataStart = HeadersContent.IndexOf("\r\n\r\n") + 4;
                    this.Stream(ByteContent.Skip(dataStart).ToArray());
                }
            }
        }

        public void Stream(byte[] bytes) {
            if (Headers.ContainsKey("Content-length") && CurrentBodySize + bytes.Length > Int32.Parse(Headers["Content-length"])) {
                bytes = bytes.Take(Int32.Parse(Headers["Content-length"]) - CurrentBodySize).ToArray();
            }
            CurrentBodySize += bytes.Length;
            this.BodyStream.Post(bytes);
        }

        public Boolean HasHeaders()
        {
            return this.HeadersContent.Contains("\r\n\r\n");
        }

        public Boolean IsComplete()
        {
            int parsed;
            return this.HeadersContent.Contains("\r\n\r\n") && (
                       !Headers.ContainsKey("Content-length") || 
                       (Int32.TryParse(Headers["Content-length"], out parsed) && parsed == CurrentBodySize)
            );
        }

        public Boolean IsClosableConnection()
        {
            return Headers.ContainsKey("Connection") && Headers["Connection"].ToLower().Equals("close");
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
                    headers.Add(line.Substring(0, index), line.Substring(index + 1).TrimStart());
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
                return content.Skip(contentStart).ToArray();
            }
        }

    }
}
