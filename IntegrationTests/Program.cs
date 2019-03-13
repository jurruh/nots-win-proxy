using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace IntegrationTests
{
    class Program
    {

        static async Task Main(string[] args)
        {
            Console.WriteLine("#########: Basic proxy tests");
            await TestBasicProxyResult();

            Console.WriteLine("\n#########: Privacy modus");
            await TestPrivacyModus();

            Console.WriteLine("\n#########: Caching");
            await TestCaching();

            Console.ReadKey();
        }

        public static async Task TestBasicProxyResult() {
            var client = SetupProxy(new Proxy.Settings() { Port = 9000 });
            string result = "My result that is long enough .................................................................";

            var ws = SetupHttpServer(9001, (HttpListenerRequest req) =>
            {
                return result;
            });

            var response = await client.SendAsync(new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost.:9001"),
                Version = HttpVersion.Version10
            });

            Assert("Basic proxying works", result, await (response).Content.ReadAsStringAsync());
        }

        public static async Task TestPrivacyModus()
        {
            var client = SetupProxy(new Proxy.Settings() { Port = 9002, PrivacyModusEnabled = true });
            string result = "My result that is long enough .................................................................";

            String useragent = ""; 
            var ws = SetupHttpServer(9003, (HttpListenerRequest req) =>
            {
                useragent = req.UserAgent;
                return result;
            });

            var response = await client.SendAsync(new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost.:9003"),
                Version = HttpVersion.Version10
            });

            Assert("Basic proxying with privacy filter works", result, await (response).Content.ReadAsStringAsync());
            Assert("User-Agent header set to Anonymous", "Anonymous", useragent);

            IEnumerable<string> values;
            string serverHeader = string.Empty;
            if (response.Headers.TryGetValues("Server", out values))
            {
                serverHeader = values.FirstOrDefault();
            }

            Assert("Server header set to Anonymous", "Anonymous", serverHeader);
        }

        public static async Task TestCaching() {
            var client = SetupProxy(new Proxy.Settings() { Port = 9004, CachingEnabled = true });
            string result = "My result that is long enough .................................................................";

            int amountIncomingRequests = 0;
            String useragent = "";
            var ws = SetupHttpServer(9005, (HttpListenerRequest req) =>
            {
                useragent = req.UserAgent;
                amountIncomingRequests++;
                return result;
            });

            var response1 = await client.SendAsync(new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost.:9005"),
                Version = HttpVersion.Version10
            });

            Assert("Basic proxying with caching 1st time works", result, await (response1).Content.ReadAsStringAsync());

            var response2 = await client.SendAsync(new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost.:9005"),
                Version = HttpVersion.Version10
            });

            Assert("Basic proxying with caching 2nd time works", result, await (response2).Content.ReadAsStringAsync());
            AssertTrue("Server only got 1 request", amountIncomingRequests == 1);
        }

        public static void AssertTrue(string name, Boolean b) {
            if (b)
            {
                Console.WriteLine($"OK: {name}");
            }
            else {
                Console.WriteLine($"Failed: {name}");
            }
        }

        public static void Assert(string name, string expected, string current) {
            AssertTrue(name, expected.Equals(current));
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        static HttpClient SetupProxy(Proxy.Settings settings) {
            settings.TestMode = true;

            Proxy.Proxy proxy = new Proxy.Proxy(settings);
            proxy.Start();

            var webProxy = new WebProxy()
            {
                Address = new Uri($"http://localhost:{settings.Port}"),
                BypassProxyOnLocal = false
            };
            WebRequest.DefaultWebProxy = webProxy;

            var httpClientHandler = new HttpClientHandler()
            {
                Proxy = webProxy,
            };

            return new HttpClient(handler: httpClientHandler, disposeHandler: true);
        }

        static WebServer SetupHttpServer(int port, Func<HttpListenerRequest, string> request)
        {
            WebServer ws = new WebServer(request, $"http://localhost:{port}/");

            ws.Run();

            return ws;
        }
    }
}
