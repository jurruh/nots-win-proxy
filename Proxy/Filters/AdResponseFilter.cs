using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Http;

namespace Proxy
{
    public class AdResponseFilter : Filter<Http.Response>
    {
        public override Response Apply(Response response)
        {
            //Block all images
            if (response.Headers.ContainsKey("Content-type") && response.Headers["Content-type"].ToLower().Contains("image"))
            {
                //response.Body = new byte[0]; todo fix
            }

            return response;
        }
    }
}
