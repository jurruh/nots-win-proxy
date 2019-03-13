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
        public static int Buffersize = 1024;

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
            // First load headers
            while (!result.HasHeaders())
            {
                byte[] buffer = new byte[Buffersize];
                await Stream.ReadAsync(buffer, 0, Buffersize);  
                result.Load(buffer);
            }

            // Now stream the content in a different Task
            Task.Run(async () => {
                while (true) {
                    byte[] buffer = new byte[Buffersize];
                    await Stream.ReadAsync(buffer, 0, Buffersize);
                    result.Load(buffer);

                    if (result.IsComplete())
                    {
                        result.BodyStream.Complete();
                        break; // Message complete
                    }

                }
            });

            return result;
        }

        public List<byte[]> Write(Response response)  
        {
            byte[] buffer = response.GetHeaderBytes();
            Stream.Write(buffer, 0, buffer.Length);

            int bodyBytesWritten = 0;

            List<byte[]> chunks = new List<byte[]>();

            while ((response.Headers.ContainsKey("Content-length") && bodyBytesWritten < Int32.Parse(response.Headers["Content-length"]))
            ) {
                try
                {
                    byte[] bodyBuffer = response.BodyStream.Receive();

                    Stream.Write(bodyBuffer, 0, bodyBuffer.Length);

                    chunks.Add(bodyBuffer);

                    bodyBytesWritten += bodyBuffer.Length;
                }
                catch (Exception e) {
                    // Stream closed
                    break;
                }
            }

            return chunks;
        }
    }
}
