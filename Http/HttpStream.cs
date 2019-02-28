using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Http
{
    class HttpStream
    {
        public static int Buffersize = 80000;

        private NetworkStream Stream { get; set; }

        public HttpStream(NetworkStream stream)
        {
            this.Stream = stream;
        }

        public async Task<T> ReadHttpStream<T>() where T : Message, new()
        {
            byte[] buffer = new byte[Buffersize];

            while (true) // Infinite loop for reading the buffer
            {
                await Stream.ReadAsync(buffer, 0, Buffersize);

                if (buffer.Where(x => x == 0).ToList().Count == buffer.Length)
                {
                    break; // client disconnected
                }

                //TODO add length for buffersize
                break;
            }

            var d = new T();

            d.Load(buffer);

            return d;
        }

        public async Task Write(Response response)
        {
            byte[] buffer = response.GetBytes();

            await Stream.WriteAsync(buffer, 0, buffer.Length);
        }
    }
}
