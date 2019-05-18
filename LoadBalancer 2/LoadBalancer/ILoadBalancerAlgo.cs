using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadBalancer
{
    public interface ILoadBalancerAlgo
    {
        void InitServers(List<Server> servers);

        Server GetServer(Http.Request request);
    }
}
