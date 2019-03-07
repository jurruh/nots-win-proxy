using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Http;

namespace Proxy.Filters
{
    public class CacheRequestFilter : Filter<Http.Request>
    {

        public override Request Apply(Request message)
        {

            return message;
        }
    }
}
