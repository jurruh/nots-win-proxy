using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Http
{
    class HttpStream
    {
        public static int Buffersize = 1;

        private NetworkStream Stream { get; set; }

        public HttpStream(NetworkStream stream)
        {
            this.Stream = stream;
        }

        public event EventHandler BodyComplete;

        public async Task<T> ReadHttpStream<T>() where T : Message, new()
        {
            List<byte> allBytes = new List<byte>();

            T result = null;
            result = new T();
            while (!result.HasHeaders()) // First load headers
            {
                byte[] buffer = new byte[Buffersize];
                await Stream.ReadAsync(buffer, 0, Buffersize);  
                result.Load(buffer);
            }

            // Now stream the content on a different Task
            Task.Run(async () => {
                while (true) {
                    byte[] buffer = new byte[Buffersize];
                    await Stream.ReadAsync(buffer, 0, Buffersize);
                    result.Load(buffer);

                    if (buffer.Where(x => x == 0).ToList().Count == buffer.Length)
                    {
                        break; // client disconnected
                    }

                }
            });

            return result;
        }

        public void Write(Response response)  
        {
            byte[] buffer = response.GetHeaderBytes();
            Stream.Write(buffer, 0, buffer.Length);

            int bodyBytesWritten = 0;

            while (
                (response.Headers.ContainsKey("Content-length") && bodyBytesWritten < Int32.Parse(response.Headers["Content-length"])) ||
                (!response.Headers.ContainsKey("Content-length"))
            ) {
                try
                {
                    byte[] bodyBuffer = response.BodyStream.Receive();

                    if (bodyBuffer.Length > 0 && bodyBuffer.Where(x => x == 0).ToList().Count == bodyBuffer.Length)
                    {
                        break; // connection closed at this point
                    }

                    Stream.Write(bodyBuffer, 0, bodyBuffer.Length);

                    bodyBytesWritten += bodyBuffer.Length;
                }
                catch (Exception e) {
                    // Stream closed
                    break;
                }
            }
        }
    }
}
