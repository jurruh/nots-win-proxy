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
        public static int Buffersize = 1024;

        private NetworkStream Stream { get; set; }

        public HttpStream(NetworkStream stream)
        {
            this.Stream = stream;
        }

        public async Task<T> ReadHttpStream<T>() where T : Message, new()
        {
            List<byte> allBytes = new List<byte>();

            T result = null;
            while (true) // Infinite loop for reading the buffer
            {
                byte[] buffer = new byte[Buffersize];
                await Stream.ReadAsync(buffer, 0, Buffersize);

                if (buffer.Where(x => x == 0).ToList().Count == buffer.Length)
                {
                    break; // client disconnected
                }

                allBytes = allBytes.Concat(buffer).ToList();
                
                result = new T();
                result.Load(allBytes.ToArray());
                if (result.IsComplete() && !result.IsClosableConnection())
                {
                    break;
                }
            }

            return result;
        }

        public async Task Write(Response response)
        {
            byte[] buffer = response.GetBytes();

            await Stream.WriteAsync(buffer, 0, buffer.Length);
        }
    }
}
