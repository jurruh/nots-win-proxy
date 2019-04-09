using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Http;

namespace LoadBalancer.Algo
{
    public class CookiePersistence : RoundRobin, IRegisterAndModifyResponse
    {
        private ConcurrentDictionary<string, Server> cache = new ConcurrentDictionary<string, Server>();

        private const string SESSION_COOKIE = "sessionid";

        private string GetSessionId(Http.Request request) {

            if (!request.Headers.ContainsKey("Cookie")) {
                return null;
            }

            var splittedCookie = request.Headers["Cookie"].Split(';');

            foreach (var cookie in splittedCookie) {
                var splitted = cookie.Split('=');

                if (splitted.Length > 1) {
                    var key = splitted[0];
                    var value = splitted[1];

                    if (key == SESSION_COOKIE) {
                        return value;
                    }
                }
            }

            return null;
        }

        public override Server GetServer(Http.Request request)
        {
            var sessionId = GetSessionId(request);

            if (sessionId == null) {
                return base.GetServer(request);
            }

            if (cache.ContainsKey(sessionId))
            {
                var cachedServer = cache[sessionId];

                if (!servers.Contains(cachedServer)) {
                    request.AbortResponse = ResponseFactory.MakeBadGateWayResponse();
                    return null;
                }

                return cachedServer;
            }

            var server = base.GetServer(request);

            return server;
        }

        public Response RegisterAndModifyResponse(Server server, Http.Request request, Response response)
        {
            var sessionId = GetSessionId(request);

            if (sessionId == null) {
                sessionId = System.Guid.NewGuid().ToString();
                response.Headers["Set-Cookie"] = $"{SESSION_COOKIE}={sessionId}";
            }

            cache[sessionId] = server;

            return response;
        }
    }
}
