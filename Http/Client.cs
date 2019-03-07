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

        public Client(String host, int port)
        {
            this.Host = host;
            this.Port = port;
        }

        public async Task<Response> Get(Request request)
        {
            try
            {
                using (var client = new TcpClient(Host, Port))
                {
                    using (var stream = client.GetStream())
                    {
                        var httpStream = new HttpStream(stream);

                        //Todo remove encoding ascii
                        var s = request.ToString();
                        var bytes = Encoding.ASCII.GetBytes(request.ToString());
                        await stream.WriteAsync(bytes, 0, bytes.Length);

                        return await httpStream.ReadHttpStream<Response>();
                    }
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
