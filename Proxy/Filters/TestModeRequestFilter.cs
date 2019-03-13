using Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proxy.Filters
{
    class TestModeRequestFilter : Filter<Request>
    {
        public override Request Apply(Request message)
        {
            message.Headers["Host"] = "localhost";
            return message;
        }
    }
}
