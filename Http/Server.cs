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

        public Server(int port)
        {
            this.Port = port;
        }

        public void Stop()
        {
            tcpListener.Stop();
        }

        public void Start()
        {
            tcpListener = new TcpListener(IPAddress.Any, Port);

            tcpListener.Start();

            Task.Run(async () => {
                while (true)
                {
                    var client = await tcpListener.AcceptTcpClientAsync();

                    Task.Run(async () =>
                    {
                        var stream = client.GetStream();
                        var httpStream = new HttpStream(stream);

                        var request = await httpStream.ReadHttpStream<Request>();

                        if (request != null && request.Uri.Port != -1 && request.Uri.Port != 443)
                        {
                            RequestReceived?.Invoke(this,
                                new RequestEventArgs(request, async response =>
                                {
                                    httpStream.Write(response);
                                    stream.Close();
                                }));
                        }
                    });

                }
            });

        }
    }
}
