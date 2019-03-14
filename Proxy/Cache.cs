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

        public Dictionary<string, Http.Response> ResponseCache { get; set; }
        public Dictionary<string, List<byte[]>> ResponseBodyCache { get; set; }

        public Cache()
        {
            this.ResponseCache = new Dictionary<string, Http.Response>();
            this.ResponseBodyCache = new Dictionary<string, List<byte[]>>();
        }

        public void Add(Http.Request request, Http.Response response, List<byte[]> bytes)
        {
            ResponseCache.Add(request.ToString(), response);
            ResponseBodyCache.Add(request.ToString(), bytes);
        }

        public Boolean IsCached(Http.Request request)
        {
            return ResponseCache.ContainsKey(request.ToString()) && ResponseCache[request.ToString()] != null;
        }

        public Http.Response Get(Http.Request request)
        {
            var response = ResponseCache[request.ToString()].Clone();

            response.BodyBuffer = new BufferBlock<byte[]>();

            Task.Run(() => {
                var cached = ResponseBodyCache[request.ToString()];

                foreach(var chunks in cached) {
                    response.BodyBuffer.Post(chunks);
                }
            });

            return response;
        }

    }
}
