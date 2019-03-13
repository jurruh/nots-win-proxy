using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Proxy
{
    class Cache
    {

        public Dictionary<string, Http.Response> dictionary { get; set; }
        public Dictionary<string, List<byte[]>> body { get; set; }

        public Cache()
        {
            this.dictionary = new Dictionary<string, Http.Response>();
            this.body = new Dictionary<string, List<byte[]>>();
        }

        public void Add(Http.Request request, Http.Response response, List<byte[]> bytes)
        {
            dictionary.Add(request.ToString(), response);
            body.Add(request.ToString(), bytes);
        }

        public Boolean IsCached(Http.Request request)
        {
            return dictionary.ContainsKey(request.ToString()) && dictionary[request.ToString()] != null;
        }

        public Http.Response Get(Http.Request request)
        {
            var response = dictionary[request.ToString()];

            response.BodyStream = new BufferBlock<byte[]>();

            Task.Run(() => {
                var cached = body[request.ToString()];

                foreach(var chunks in cached) {
                    response.BodyStream.Post(chunks);
                }
            });

            return response;
        }

    }
}
