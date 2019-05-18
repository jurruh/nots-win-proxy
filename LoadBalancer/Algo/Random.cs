using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadBalancer.Algo
{
    class Random : ILoadBalancerAlgo
    {

        static System.Random random = new System.Random();

        protected List<Server> servers;

        public virtual Server GetServer(Http.Request request)
        {

            return servers[random.Next(servers.Count)];
        }

        public void InitServers(List<Server> servers)
        {
            this.servers = servers;
        }
    }
}
