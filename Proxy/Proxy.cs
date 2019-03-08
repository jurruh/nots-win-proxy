using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Http;
using Proxy.Filters;

namespace Proxy
{
    public class Proxy
    {
        private int Port { get; set; }

        private List<Filter<Request>> requestFilters;
        private List<Filter<Response>> responseFilters;

        private Cache Cache { get; set; }

        public Proxy(int Port)
        {
            this.Port = Port;
            this.Cache = new Cache();

            this.requestFilters = new List<Filter<Request>>();
            this.responseFilters = new List<Filter<Response>>();

            // If there is time, this can be done using reflection
            responseFilters.Add(new AdResponseFilter());
            requestFilters.Add(new BasicAuthenticationRequestFilter());
            requestFilters.Add(new PrivacyRequestFilter());
        }

        public async Task Start()
        {
            var server = new Server(Port);

            server.RequestReceived += async (sender, args) =>
            {
                var request = args.Request;

                if (ApplyRequestFilters(args, ref request)) return;

                if (Cache.IsCached(request)) {
                    args.ResponseAction(Cache.Get(request));
                    return;
                }

                var httpClient = new Http.Client(request.Uri.Host, request.Uri.Port);
                var response = await httpClient.Get(request);

                if (ApplyResponseFilters(args, ref response)) return;

                if (response != null)
                {
                    Cache.Add(request, response);
                    args.ResponseAction(response);
                }
            };

            await server.Start();
        }

        private bool ApplyResponseFilters(RequestEventArgs args, ref Response response)
        {
            foreach (var filter in responseFilters)
            {
                response = filter.Apply(response);

                if (filter.AbortResponse != null)
                {
                    args.ResponseAction(filter.AbortResponse);
                    return true;
                }
            }

            return false;
        }

        private bool ApplyRequestFilters(RequestEventArgs args, ref Request request)
        {
            foreach (var filter in requestFilters)
            {
                request = filter.Apply(request);

                if (filter.AbortResponse != null)
                {
                    args.ResponseAction(filter.AbortResponse);
                    return true;
                }
            }

            return false;
        }
    }
}