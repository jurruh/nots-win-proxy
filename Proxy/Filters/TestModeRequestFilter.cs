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
            if (message.Headers.ContainsKey("host")) {
                message.Headers["Host"] = message.Headers["Host"].Replace("localhost.", "localhost");
            }

            return message;
        }
    }
}
