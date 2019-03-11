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

        public event EventHandler<RequestEventArgs> RequestReceivedFromClient;
        public event EventHandler<RequestEventArgs> RequestSendToExternalServer;
        public event EventHandler<ResponseEventArgs> ResponseFromExternalServer;
        public event EventHandler<ResponseEventArgs> ResponseSendToClient;

        public Proxy(Settings settings)
        {
            this.Port = settings.Port;

            if (settings.CachingEnabled)
            {
                this.Cache = new Cache();
            }

            this.requestFilters = new List<Filter<Request>>();
            this.responseFilters = new List<Filter<Response>>();

            if (settings.AdBlockerEnabled) {
                responseFilters.Add(new AdResponseFilter());
            }

            if (settings.AuthenticationEnabled) {
                requestFilters.Add(new BasicAuthenticationRequestFilter());
            }

            if (settings.PrivacyModusEnabled) {
                responseFilters.Add(new PrivacyResponseFilter());
                requestFilters.Add(new PrivacyRequestFilter());
            }
        }

        public async Task Start()
        {
            var server = new Server(Port);

            server.RequestReceived += async (sender, args) =>
            {
                RequestReceivedFromClient?.Invoke(this, new RequestEventArgs(args.Request));
                var request = args.Request;

                if (ApplyRequestFilters(args, ref request)) return;

                if (Cache != null && Cache.IsCached(request)) {
                    args.ResponseAction(Cache.Get(request));
                    return;
                }

                RequestReceivedFromClient?.Invoke(this, new RequestEventArgs(request));
                var httpClient = new Http.Client(request.Uri.Host, request.Uri.Port);
                var response = await httpClient.Get(request);
                ResponseFromExternalServer?.Invoke(this, new ResponseEventArgs(response));

                if (ApplyResponseFilters(args, ref response)) return;

                if (response != null)
                {
                    Cache?.Add(request, response);
                    args.ResponseAction(response);
                    ResponseSendToClient?.Invoke(this, new ResponseEventArgs(response));
                }
            };

            await server.Start();
        }

        private bool ApplyResponseFilters(Http.RequestEventArgs args, ref Response response)
        {
            foreach (var filter in responseFilters)
            {
                response = filter.Apply(response);

                if (filter.AbortResponse != null)
                {
                    args.ResponseAction(filter.AbortResponse);
                    ResponseSendToClient?.Invoke(this, new ResponseEventArgs(filter.AbortResponse));
                    return true;
                }
            }

            return false;
        }

        private bool ApplyRequestFilters(Http.RequestEventArgs args, ref Request request)
        {
            foreach (var filter in requestFilters)
            {
                request = filter.Apply(request);

                if (filter.AbortResponse != null)
                {
                    args.ResponseAction(filter.AbortResponse);
                    ResponseSendToClient?.Invoke(this, new ResponseEventArgs(filter.AbortResponse));
                    return true;
                }
            }

            return false;
        }
    }
}