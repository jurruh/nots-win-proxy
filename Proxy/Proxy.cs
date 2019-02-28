using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Http;

namespace Proxy
{
    public class Proxy
    {
        private int Port { get; set; }

        public Proxy(int Port)
        {
            this.Port = Port;
        }

        public async Task Start()
        {
            var server = new Server(Port);

            server.RequestReceived += async (sender, args) =>
            {
                var httpClient = new Http.Client(args.Request.ResolveToAddress(), 80);

                var response = await httpClient.Get(args.Request);

                if (response != null)
                {
                    args.ResponseAction(response);
                }
                else
                {
                    // TODO
                }

            };

            await server.Start();
        }
    }
}