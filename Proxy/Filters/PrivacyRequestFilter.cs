using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Http;

namespace Proxy
{
    class PrivacyRequestFilter : Filter<Request>
    {
        public override Request Apply(Request request)
        {
            request.Headers["User-agent"] = "Anonymous";

            return request;
        }
    }
}
