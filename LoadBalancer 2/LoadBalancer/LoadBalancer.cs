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

        private Http.Server httpServer;

        public event EventHandler<LogEventArgs> OnLog;


        public LoadBalancer(Configuration configuration)
        {
            Configuration = configuration;

        }

        public void Log(String s) {
            OnLog?.Invoke(this, new LogEventArgs(s));
        }

        public async Task DoHealthCheck() {
            var servers = new List<Server>();

            Log("## - Performing healthcheck - ##");

            foreach (var server in Configuration.Servers) {
                var check = new HealthCheck(server, Configuration.MaxTimeout, Configuration.BufferSize);

                if (await check.IsOk())
                {
                    Log($"{server} is healthy");
                    servers.Add(server);
                }
                else {
                    Log($"{server} is unhealthy");
                }
            }

            Configuration.LoadBalancerAlgo.InitServers(servers);
        }

        public void Stop() {
            httpServer.Stop();
            IsRunning = false;
        }

        public async Task Start() {
            IsRunning = true;

            await DoHealthCheck();

            Task.Run(async () => {
                while (IsRunning)
                {
                    await Task.Delay(Configuration.HealthCheckInterval);
                    await DoHealthCheck();
                }
            });

            httpServer = new Http.Server(Configuration.Port, Configuration.BufferSize);

            httpServer.Start();

            httpServer.RequestReceived += async (sender, e) =>
            {
                Log($"## - Incoming request - ##");

                var server = Configuration.LoadBalancerAlgo.GetServer(e.Request);

                var response = e.Request.AbortResponse;

                if (response == null)
                {
                    Log($"Sending request to {server}: {e.Request}");

                    var httpClient = new Http.Client(server.Endpoint, server.Port, Configuration.BufferSize);

                    e.Request.Headers["Host"] = server.Endpoint;

                    var task =  httpClient.Get(e.Request);

                    if (await Task.WhenAny(task, Task.Delay(Configuration.MaxTimeout)) == task && task.Result != null)
                    {
                        response = task.Result;
                    }
                    else {
                        response = ResponseFactory.MakeGateWayTimeoutResponse();
                        Log($"Aborting with status: {response.Status} {response.StatusMessage}");
                    }

                }
                else
                {
                    Log($"Aborting with status: {response.Status} {response.StatusMessage}");
                }

                if (Configuration.LoadBalancerAlgo is IRegisterAndModifyResponse) {
                    ((IRegisterAndModifyResponse)Configuration.LoadBalancerAlgo).RegisterAndModifyResponse(server, e.Request, response);
                }

                Log($"Outgoing response: {Encoding.ASCII.GetString(response.GetHeaderBytes())}");

                e.ResponseAction(response);
            };
        }
    }
}
