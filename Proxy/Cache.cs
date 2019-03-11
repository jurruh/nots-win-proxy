using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proxy
{
    class Cache
    {

        public Dictionary<string, Http.Response> dictionary { get; set; }

        public Cache()
        {
            this.dictionary = new Dictionary<string, Http.Response>();
        }

        public void Add(Http.Request request, Http.Response response)
        {
            dictionary.Add(request.ToString(), response);
        }

        public Boolean IsCached(Http.Request request)
        {
            return dictionary.ContainsKey(request.ToString()) && dictionary[request.ToString()] != null;
        }

        public Http.Response Get(Http.Request request)
        {
            return dictionary[request.ToString()];
        }

    }
}
