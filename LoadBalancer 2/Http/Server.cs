using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Http
{
    public class Server
    {
        public event EventHandler<RequestEventArgs> RequestReceived;

        private TcpListener tcpListener;

        private int Port { get; set; }

        private int Buffersize { get; set; }

        public bool IsRunning { get; set; }

        public Server(int port, int buffersize)
        {
            this.Port = port;
            this.Buffersize = buffersize;
        }

        public void Stop()
        {
            IsRunning = false;
            tcpListener.Stop();
        }

        public void Start()
        {
            IsRunning = true;

            tcpListener = new TcpListener(IPAddress.Any, Port);

            tcpListener.Start();

            Task.Run(async () => {
                while (true)
                {
                    var client = await tcpListener.AcceptTcpClientAsync();

                    Task.Run(async () =>
                    {
                        var stream = client.GetStream();
                        var httpStream = new HttpStream(stream, Buffersize);

                        var request = await httpStream.ReadHttpStream<Request>();

                        if (request != null)
                        {
                            request.Sender = client;
                            RequestReceived?.Invoke(this,
                                new RequestEventArgs(request, response =>
                                {
                                    var bytes = httpStream.Write(response);
                                    stream.Close();

                                    return bytes;
                                }));
                        }
                    });

                }
            });

        }
    }
}
