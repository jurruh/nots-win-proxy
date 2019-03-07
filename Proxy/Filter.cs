using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Http;

namespace Proxy
{
    public abstract class Filter<T> where T : Http.Message
    {
        public Response AbortResponse { get; set; }

        public abstract T Apply(T message);
    }
}
