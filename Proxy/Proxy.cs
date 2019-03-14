using Http;
using Proxy.Filters;
using System;
using System.Collections.Generic;

namespace Proxy
{
    public class Proxy
    {

        private Cache Cache { get; set; }
        private Server Server { get; set; }

        public event EventHandler<RequestEventArgs> RequestReceivedFromClient;
        public event EventHandler<RequestEventArgs> RequestSendToExternalServer;
        public event EventHandler<ResponseEventArgs> ResponseFromExternalServer;
        public event EventHandler<ResponseEventArgs> ResponseSendToClient;

        private Configuration Configuration { get; set; }

        public Proxy(Configuration settings)
        {

            this.Configuration = settings;

            if (settings.CachingEnabled)
            {
                this.Cache = new Cache();
            }
        }

        private List<Filter<Request>> GetRequestFilters() {
            var requestFilters = new List<Filter<Request>>();

            if (Configuration.AuthenticationEnabled)
            {
                requestFilters.Add(new BasicAuthenticationRequestFilter());
            }

            if (Configuration.PrivacyModusEnabled)
            {
                requestFilters.Add(new PrivacyRequestFilter());
            }

            if (Configuration.TestMode)
            {
                requestFilters.Add(new TestModeRequestFilter());
            }

            return requestFilters;
        }

        private List<Filter<Response>> GetResponseFilters() {
            var responsefilters = new List<Filter<Response>>();

            if (Configuration.AdBlockerEnabled)
            {
                responsefilters.Add(new AdResponseFilter());
            }

            if (Configuration.PrivacyModusEnabled)
            {
                responsefilters.Add(new PrivacyResponseFilter());
            }

            return responsefilters;
        }

        public void Stop()
        {
            Server.Stop();
        }

        public void Start()
        {
            Server = new Server(Configuration.Port, Configuration.BufferSize);

            Server.RequestReceived += async (sender, args) =>
            {
                RequestReceivedFromClient?.Invoke(this, new RequestEventArgs(args.Request));
                var request = args.Request;

                if (ApplyRequestFilters(args, ref request)) return;

                if (Cache != null && Cache.IsCached(request)) {
                    args.ResponseAction(Cache.Get(request));
                    return;
                }

                RequestSendToExternalServer?.Invoke(this, new RequestEventArgs(request));

                var httpClient = new Http.Client(request.Uri.Host, request.Uri.Port, Configuration.BufferSize);
                var response = await httpClient.Get(request);
                ResponseFromExternalServer?.Invoke(this, new ResponseEventArgs(response));

                if (ApplyResponseFilters(args, ref response)) return;

                if (response != null)
                {
                    var bytes = args.ResponseAction(response);
                    Cache?.Add(request, response, bytes);
                    ResponseSendToClient?.Invoke(this, new ResponseEventArgs(response));
                }
            };

            Server.Start();
        }

        private bool ApplyResponseFilters(Http.RequestEventArgs args, ref Response response)
        {
            var responseFilters = GetResponseFilters();

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
            var requestFilters = GetRequestFilters();

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