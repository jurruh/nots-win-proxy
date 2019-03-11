using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proxy
{
    public class ResponseEventArgs
    {
        public Http.Response Response { get; set; }

        public ResponseEventArgs(Http.Response response)
        {
            this.Response = response;
        }
    }
}
