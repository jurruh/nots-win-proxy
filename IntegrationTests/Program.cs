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
            await TestBasicProxyResult();

            Console.ReadKey();
        }

        public static async Task TestBasicProxyResult() {
            var client = SetupProxy(new Proxy.Settings() { Port = 9000 });
            string result = "My result that is long enough .................................................................";
            var ws = SetupHttpServer(9001, result);

            var request = await client.SendAsync(new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost.:9001"),
                Version = HttpVersion.Version10
            });

            Assert("Basic proxying works", result, await (request).Content.ReadAsStringAsync());
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
            if (expected.Equals(current))
            {
                Console.WriteLine($"OK: {name}");
            }
            else {
                Console.WriteLine($"FAILED: {name}");
            }
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

        static WebServer SetupHttpServer(int port, string content)
        {
            WebServer ws = new WebServer((HttpListenerRequest request) =>
            {
                return content;
            }, $"http://localhost:{port}/");

            ws.Run();

            return ws;
        }
    }
}
