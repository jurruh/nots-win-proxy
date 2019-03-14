using System;
using System.Collections.Generic;
using System.IO;
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
            string result = "My result that is long enough .................................................................";

            Console.WriteLine("#########: Basic proxy tests");
            await TestBasicProxyResult();

            Console.WriteLine("\n#########: Authentication");
            await TestAuthentication();

            Console.WriteLine("\n#########: Privacy modus");
            await TestPrivacyModus();

            Console.WriteLine("\n#########: Caching");
            await TestCaching();

            Console.WriteLine("\n#########: Ad blocker test");
            await TestAdBlocker();

            Console.WriteLine("\n#########: Test images");
            await TestImages();

            Console.WriteLine("\n!Press any key to start stress tests");
            Console.ReadKey();

            Console.WriteLine("\n#########: Test simultaneous requests");
            await TestSimultaneousRequests();
            Console.ReadKey();
        }

        public static async Task TestBasicProxyResult() {
            var client = SetupProxy(new Proxy.Configuration() { Port = 9000 });
            string result = "My result that is long enough .................................................................";

            var ws = SetupHttpServer(9001, (HttpListenerContext req) =>
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
            var client = SetupProxy(new Proxy.Configuration() { Port = 9002, PrivacyModusEnabled = true });
            string result = "My result that is long enough .................................................................";

            String useragent = ""; 
            var ws = SetupHttpServer(9003, (HttpListenerContext req) =>
            {
                useragent = req.Request.UserAgent;
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
            var client = SetupProxy(new Proxy.Configuration() { Port = 9004, CachingEnabled = true });
            string result = "My result that is long enough .................................................................";

            int amountIncomingRequests = 0;
            String useragent = "";
            var ws = SetupHttpServer(9005, (HttpListenerContext req) =>
            {
                useragent = req.Request.UserAgent;
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

            var response3 = await client.SendAsync(new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost.:9005"),
                Version = HttpVersion.Version10
            });
            Assert("Basic proxying with caching 3rd time works", result, await (response3).Content.ReadAsStringAsync());

            AssertTrue("Server only got 1 request", amountIncomingRequests == 1);
        }

        public static async Task TestAuthentication()
        {
            var client = SetupProxy(new Proxy.Configuration { Port = 9006, AuthenticationEnabled = true });
            string result = "My result that is long enough .................................................................";

            var ws = SetupHttpServer(9007, (HttpListenerContext req) =>
            {
                return result;
            });

            var response1 = await client.SendAsync(new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost.:9007"),
                Version = HttpVersion.Version10
            });

            AssertTrue("Authentication returns status 407 when no credentials", response1.StatusCode == HttpStatusCode.ProxyAuthenticationRequired);

            client.DefaultRequestHeaders.ProxyAuthorization = new AuthenticationHeaderValue("Basic", Base64Encode("admin:admin"));

            var response2 = await client.SendAsync(new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost.:9007"),
                Version = HttpVersion.Version10
            });

            Assert("Proxy works with authentication", result, await (response2).Content.ReadAsStringAsync());
        }

        public static async Task TestAdBlocker()
        {
            var client = SetupProxy(new Proxy.Configuration { Port = 9008, AdBlockerEnabled = true });

            string result = "My result that is long enough .................................................................";

            var ws = SetupHttpServer(9009, (HttpListenerContext req) =>
            {
                req.Response.AddHeader("Content-type", "image/gif");

                return result;
            });

            var response1 = await client.SendAsync(new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost.:9009"),
                Version = HttpVersion.Version10
            });

            Assert("Images are empty", "", await (response1).Content.ReadAsStringAsync());
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

        public static async Task TestImages()
        {
            var client = SetupProxy(new Proxy.Configuration() { Port = 9010 });
            var file = File.ReadAllBytes("test.jpeg");

            var ws = SetupHttpServer(9011, (HttpListenerContext req) =>
            {
                req.Response.Headers.Add("Content-type", "image/jpeg");
                req.Response.ContentLength64 = file.Length;
                req.Response.OutputStream.Write(file, 0, file.Length);

                return "";
            });

            var response = await client.SendAsync(new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost.:9011"),
                Version = HttpVersion.Version10
            });

            var x = await response.Content.ReadAsByteArrayAsync();

            AssertTrue("Image from proxy same size as filesystem", x.Length == file.Length);
            AssertTrue("Image from proxy same content as filesystem", file.SequenceEqual(x));
        }

        public static async Task TestSimultaneousRequests()
        {
            var client = SetupProxy(new Proxy.Configuration() { Port = 9012 });

            string result = "My result that is long enough .................................................................";

            var ws = SetupHttpServer(9013, (HttpListenerContext req) =>
            {
                return result;
            });

            for (int x = 0; x < 100; x++)
            {
                int current = x;
                Task.Run(async () => {
                    var response = await client.SendAsync(new HttpRequestMessage
                    {
                        RequestUri = new Uri("http://localhost.:9013"),
                        Version = HttpVersion.Version10
                    });

                    Assert($"Simultanous request {current}", result, await (response).Content.ReadAsStringAsync());
                });
            }
        }

        static HttpClient SetupProxy(Proxy.Configuration settings) {
            settings.TestMode = true;

            Proxy.Proxy proxy = new Proxy.Proxy(settings);
            proxy.Start();
            Console.WriteLine("Proxy running...");

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

        static WebServer SetupHttpServer(int port, Func<HttpListenerContext, string> request)
        {
            WebServer ws = new WebServer(request, $"http://localhost:{port}/");

            ws.Run();

            return ws;
        }
    }
}
