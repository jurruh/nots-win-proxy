using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proxy
{
    class CachedResponse : Http.Response
    {
        public byte[] CachedBody { get; set; }
    }
}
