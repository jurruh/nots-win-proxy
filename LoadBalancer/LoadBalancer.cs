using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadBalancer
{
    public class LoadBalancer
    {
        public Configuration Configuration { get; set; }

        public Boolean IsRunning { get; set; } = false;

        public List<Server> HealthyServers { get; set; }  = new List<Server>();

        public LoadBalancer(Configuration configuration)
        {
            Configuration = configuration;

        }

        public async Task DoHealthCheck() {
            var servers = new List<Server>();

            foreach (var server in Configuration.Servers) {
                var check = new HealthCheck(server, Configuration.MaxTimeout, Configuration.BufferSize);

                if (await check.IsOk()) {
                    servers.Add(server);
                }
            }

            Configuration.LoadBalancerAlgo.InitServers(servers);
        }

        public void Stop() {
            IsRunning = false;
        }

        public async Task Start() {
            IsRunning = true;

            await DoHealthCheck();

            Task.Run(async () => {
                while (IsRunning)
                {
                    Task.Delay(30000); // healthcheck every 30 seconds
                    await DoHealthCheck();
                }
            });

            var httpServer = new Http.Server(Configuration.Port, Configuration.BufferSize);

            httpServer.Start();

            httpServer.RequestReceived += async (sender, e) =>
            {
                var server = Configuration.LoadBalancerAlgo.GetServer(e.Request);

                var httpClient = new Http.Client(server.Endpoint, server.Port, Configuration.BufferSize);

                e.Request.Headers["Host"] = server.Endpoint;

                var response = await httpClient.Get(e.Request);

                e.ResponseAction(response);
            };
        }
    }
}
