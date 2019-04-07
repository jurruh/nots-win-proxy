using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadBalancer.Algo
{
    public class RoundRobin : ILoadBalancerAlgo
    {
        List<Server> servers;

        int CurrentIndex = 0;

        public virtual Server GetServer(Http.Request request)
        {
            return servers[CurrentIndex++ % servers.Count];
        }

        public void InitServers(List<Server> servers)
        {
            this.servers = servers;
        }
    }
}
