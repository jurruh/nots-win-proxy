using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadBalancer
{
    class HealthCheck
    {
        public Server Server { get; set; }

        public int MaxTimeout { get; set; }

        public int BufferSize { get; set; }


        public HealthCheck(Server server, int maxTimeout, int bufferSize)
        {
            Server = server;
            MaxTimeout = maxTimeout;
            BufferSize = bufferSize;
        }

        public async Task<bool> IsOk() {
            var httpClient = new Http.Client(Server.Endpoint, Server.Port, BufferSize);

            bool hasResponse = false;

            var task = Task.Run(async () => {
                var request = new Http.Request();

                request.Headers["Host"] = "localhost";

                var result = await httpClient.Get(request);

                hasResponse = result != null;
            });

            return await Task.WhenAny(task, Task.Delay(MaxTimeout)) == task && hasResponse;
        }

    }
}
