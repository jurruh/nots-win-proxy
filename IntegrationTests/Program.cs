using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
            var proxy = new WebProxy()
            {
                Address = new Uri($"http://localhost:8080")
            };

            var httpClientHandler = new HttpClientHandler()
            {
                Proxy = proxy,
            };

            var client = new HttpClient(handler: httpClientHandler, disposeHandler: true);


            var ws = await SetupHttpServer();

            var program = Runner.Program.Main(new string[0]);


            var request = await client.GetAsync("http://motherfuckingwebsite.com");

            Console.Write(await (request).Content.ReadAsStringAsync());

            Console.ReadKey();
            ws.Stop();
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
