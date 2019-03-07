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
            var program = Runner.Program.Main(new string[0]);

            var proxy = new WebProxy()
            {
                Address = new Uri($"http://localhost:8080")
            };
            WebRequest.DefaultWebProxy = proxy;

            var httpClientHandler = new HttpClientHandler()
            {
                Proxy = proxy,
            };

            var client = new HttpClient(handler: httpClientHandler, disposeHandler: true);

            var ws = await SetupHttpServer();

            client.DefaultRequestHeaders.ProxyAuthorization = new AuthenticationHeaderValue("Basic",Base64Encode("admin:admin"));

            var request = await client.SendAsync(new HttpRequestMessage
            {
                RequestUri = new Uri("http://motherfuckingwebsite.com"),
                Version = HttpVersion.Version10
            });

//            Console.Write(await (request).Content.ReadAsStringAsync());

            Console.ReadKey();
            ws.Stop();
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }


        static async Task<WebServer> SetupHttpServer()
        {
            WebServer ws = new WebServer((HttpListenerRequest request) =>
            {
                return string.Format("<HTML><BODY>My web page.<br>{0}</BODY></HTML>", DateTime.Now);
            }, "http://localhost:8081/");

            ws.Run();

            return ws;
        }
    }
}
