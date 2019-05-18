using LoadBalancer.Algo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadBalancer
{
    public class Configuration
    {
        public int Port { get; set; } = 8080;

        public int BufferSize { get; set; } = 1024;

        public int HealthCheckInterval { get; set; } = 30000;

        public List<Server> Servers { get; set; } = new List<Server>();

        public ILoadBalancerAlgo LoadBalancerAlgo { get; set; } = new RoundRobin();

        public int MaxTimeout { get; set; } = 5000;
    }
}
