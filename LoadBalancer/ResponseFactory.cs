using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadBalancer
{
    public static class ResponseFactory
    {
        public static Http.Response MakeGateWayTimeoutResponse() {
            var response = new Http.Response();

            response.Status = 504;
            response.StatusMessage = "Gateway timeout";

            response.Headers["Content-length"] = "0";

            return response;
        }

        public static Http.Response MakeBadGateWayResponse()
        {
            var response = new Http.Response();

            response.Status = 502;
            response.StatusMessage = "Bad gateway";

            response.Headers["Content-length"] = "0";

            return response;
        }

    }
}
