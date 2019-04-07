using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadBalancer.Algo
{
    public class CookiePersistence : RoundRobin
    {
        private ConcurrentDictionary<string, Server> cache = new ConcurrentDictionary<string, Server>();

        private string[] SESSION_COOKIES = new string[] { "sessid", "sessionid", "phpsessid" };

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

                    if (SESSION_COOKIES.Contains(key.ToLower())) {
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
                return cache[sessionId];
            }

            var server = base.GetServer(request);

            cache[sessionId] = server;

            return server;
        }

    }
}
