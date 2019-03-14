using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Http
{
    public class Client
    {
        public int Port { get; set; }
        public string Host { get; set; }
        public int Buffersize { get; set; }

        public Client(String host, int port, int buffersize)
        {
            this.Host = host;
            this.Port = port;
            this.Buffersize = buffersize;
        }

        public async Task<Response> Get(Request request)
        {
            try
            {
                var client = new TcpClient(Host, Port);
                var stream = client.GetStream();
                var httpStream = new HttpStream(stream, Buffersize);

                //Todo remove encoding ascii
                var s = request.ToString();
                var bytes = Encoding.ASCII.GetBytes(request.ToString());
                await stream.WriteAsync(bytes, 0, bytes.Length);

                httpStream.BodyComplete += (sender, args) =>
                {
                    stream.Close();
                };

                return await httpStream.ReadHttpStream<Response>();
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
