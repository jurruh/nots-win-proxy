using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LoadBalancer.Algo
{
    public class SessionTablePersistence : RoundRobin
    {
        private ConcurrentDictionary<string, Server> cache = new ConcurrentDictionary<string, Server>();

        public override Server GetServer(Http.Request request) {
            var ip = ((IPEndPoint)request.Sender.Client.RemoteEndPoint).Address.ToString();

            if (cache.ContainsKey(ip))
            {
                var cachedServer = cache[ip];

                if (!servers.Contains(cachedServer))
                {
                    request.AbortResponse = ResponseFactory.MakeBadGateWayResponse();
                    return null;
                }

                return cachedServer;
            }

            var server = base.GetServer(request);

            cache[ip] = server;

            return server;
        } 

    }
}
