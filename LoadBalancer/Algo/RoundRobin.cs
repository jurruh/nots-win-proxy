using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadBalancer.Algo
{
    public class RoundRobin : ILoadBalancerAlgo
    {
        protected List<Server> servers;

        int CurrentIndex = 0;

        public virtual Server GetServer(Http.Request request)
        {
            if (servers.Count() == 0) {
                request.AbortResponse = ResponseFactory.MakeBadGateWayResponse();
                return null;
            }

            return servers[CurrentIndex++ % servers.Count];
        }

        public void InitServers(List<Server> servers)
        {
            this.servers = servers;
        }
    }
}
