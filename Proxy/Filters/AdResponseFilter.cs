using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Http;
using System.Threading.Tasks.Dataflow;

namespace Proxy
{
    public class AdResponseFilter : Filter<Http.Response>
    {
        public override Response Apply(Response response)
        {
            //Block all images
            if (response.Headers.ContainsKey("Content-type") && response.Headers["Content-type"].ToLower().Contains("image"))
            {
                response.BodyStream = new BufferBlock<byte[]>();
                response.Headers["Content-length"] = "0";
            }

            return response;
        }
    }
}
