using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Http;

namespace Proxy
{
    class PrivacyResponseFilter : Filter<Http.Response>
    {
        public override Response Apply(Response response)
        {
            response.Headers["Server"] = "Anonymous";

            return response;
        }
    }
}
