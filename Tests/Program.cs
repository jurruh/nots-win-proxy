using LoadBalancer.Algo;
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
            Console.WriteLine("# Testing basic loadbalancing");
            await TestBasicLoadbalancing();

            Console.WriteLine("\n# Testing persistence with session cookei");
            await TestCookiePersistence();

            Console.WriteLine("\n# Testing sessiontable persistence");
            await TestServertablePersistence();

            Console.WriteLine("\n# Testing healthchecking");
            await TestOnlyHealthyServersTimeout();

            Console.WriteLine("\n# Testing health monitoring skips servers dat are down");
            await TestLoadBalancerWorksWhenServerIsDown();

            Console.ReadKey();
        }

        public static async Task TestCookiePersistence()
        {
            var servers = new List<LoadBalancer.Server>();
            servers.Add(new LoadBalancer.Server() { Endpoint = "localhost", Port = 9040 });
            var ws1 = SetupHttpServer(9040, (HttpListenerContext req) =>
            {
                return "Server 1";
            });

            servers.Add(new LoadBalancer.Server() { Endpoint = "localhost", Port = 9041 });
            var ws2 = SetupHttpServer(9041, (HttpListenerContext req) =>
            {
                return "Server 2";
            });


            var lb = await SetupLoadBalancer(new LoadBalancer.Configuration() { Port = 9044, Servers = servers, LoadBalancerAlgo = new CookiePersistence() });

            var baseAddress = new Uri("http://localhost:9044");
            var cookieContainer = new CookieContainer();
            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
            {
                var result0 = await client.GetAsync("/");
                var value = cookieContainer.GetCookies(baseAddress).Cast<Cookie>().FirstOrDefault(x => x.Name == "sessionid");
                Console.WriteLine($"Got cookie {value.Value}");
                var result1 = await client.GetStringAsync("/");
                var result2 = await client.GetStringAsync("/");
                Assert("First cookie persistence request works", "Server 1", result1);
                Assert("First cookie persistence request works", "Server 1", result2);
            }

            cookieContainer = new CookieContainer();
            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
            {
                cookieContainer.Add(baseAddress, new Cookie("sessionid", "12345"));
                var result1 = await client.GetStringAsync("/");
                var result2 = await client.GetStringAsync("/");
                Assert("Second cookie persistence request works", "Server 2", result1);
                Assert("Second cookie persistence request works", "Server 2", result2);

                servers.Last().Port = 9999; // make unavailible

                await lb.DoHealthCheckStartup();

                var result3 = await client.GetAsync("http://localhost:9044");

                AssertTrue("Returns bad gateway when server is off", result3.StatusCode.Equals(HttpStatusCode.BadGateway));
            }
        }

        public static async Task TestLoadBalancerWorksWhenServerIsDown() {
            var servers = new List<LoadBalancer.Server>();

            servers.Add(new LoadBalancer.Server() { Endpoint = "localhost", Port = 9031 }); // Does not have a server
            servers.Add(new LoadBalancer.Server() { Endpoint = "localhost", Port = 9032 });
            var ws2 = SetupHttpServer(9032, (HttpListenerContext req) =>
            {
                return "Server 2";
            });

            var client = new HttpClient();

            await SetupLoadBalancer(new LoadBalancer.Configuration() { Port = 9034, Servers = servers });
            var result1 = await client.GetStringAsync("http://localhost:9034");
            var result2 = await client.GetStringAsync("http://localhost:9034");

            Assert("Servers that are down ignored 1", result1, "Server 2");
            Assert("Servers that are down ignored 2", result2, "Server 2");
        }

        public static async Task TestOnlyHealthyServersTimeout()
        {
            var servers = new List<LoadBalancer.Server>();
            servers.Add(new LoadBalancer.Server() { Endpoint = "localhost", Port = 9020 });
            var ws1 = SetupHttpServer(9020, (HttpListenerContext req) =>
            {
                Thread.Sleep(6000);
                return "Server 1";
            });

            servers.Add(new LoadBalancer.Server() { Endpoint = "localhost", Port = 9021 });
            var ws2 = SetupHttpServer(9021, (HttpListenerContext req) =>
            {
                return "Server 2";
            });


            var client = new HttpClient();

            await SetupLoadBalancer(new LoadBalancer.Configuration() { Port = 9024, Servers = servers });
            var result1 = await client.GetStringAsync("http://localhost:9024");
            var result2 = await client.GetStringAsync("http://localhost:9024");

            Assert("Servers with a timeout > 5000ms are ignored", result1, "Server 2");
            Assert("Servers with a timeout > 5000ms are ignored", result2, "Server 2");
        }


        public static async Task TestServertablePersistence() {
            var servers = new List<LoadBalancer.Server>();
            servers.Add(new LoadBalancer.Server() { Endpoint = "localhost", Port = 9010 });
            var ws1 = SetupHttpServer(9010, (HttpListenerContext req) =>
            {
                return "Server 1";
            });

            servers.Add(new LoadBalancer.Server() { Endpoint = "localhost", Port = 9011 });
            var ws2 = SetupHttpServer(9011, (HttpListenerContext req) =>
            {
                return "Server 2";
            });


            var client = new HttpClient();

            var lb = await SetupLoadBalancer(new LoadBalancer.Configuration() { Port = 9014, Servers = servers, LoadBalancerAlgo = new SessionTablePersistence() });
            var result1 = await client.GetStringAsync("http://localhost:9014");
            var result2 = await client.GetStringAsync("http://localhost:9014");

            Assert("ServertablePersistence has result 1", result1, "Server 1");

            Assert("ServertablePersistence works", result1, result2);

            servers.First().Port = 9999; // make unavailible

            await lb.DoHealthCheckStartup();

            var result3 = await client.GetAsync("http://localhost:9014");

            AssertTrue("Returns bad gateway when server is off", result3.StatusCode.Equals(HttpStatusCode.BadGateway));

        }

        public static async Task TestBasicLoadbalancing() {
            var servers = new List<LoadBalancer.Server>();
            servers.Add(new LoadBalancer.Server() { Endpoint = "localhost", Port = 9001 });
            servers.Add(new LoadBalancer.Server() { Endpoint = "localhost", Port = 9002 });
            servers.Add(new LoadBalancer.Server() { Endpoint = "localhost", Port = 9003 });

            var doSleep = false;
            var ws1 = SetupHttpServer(9001, (HttpListenerContext req) =>
            {
                //if (doSleep) {
                  //  Thread.Sleep(6000);
                //}

                return "Server 1";
            });

            var ws2 = SetupHttpServer(9002, (HttpListenerContext req) =>
            {
                return "Server 2";
            });

            var ws3 = SetupHttpServer(9003, (HttpListenerContext req) =>
            {
                return "Server 3";
            });

            await SetupLoadBalancer(new LoadBalancer.Configuration() { Port = 9000, Servers = servers });

            var client = new HttpClient();

            var result1 = await client.GetStringAsync("http://localhost:9000");
            Assert("Round Robin first request works", "Server 1" ,result1);

            var result2 = await client.GetStringAsync("http://localhost:9000");
            Assert("Round Robin second request works", "Server 2", result2);

            var result3 = await client.GetStringAsync("http://localhost:9000");
            Assert("Round Robin third request works", "Server 3", result3);

            doSleep = true;

            var result4 = await client.GetAsync("http://localhost:9000");
            AssertTrue("Returns gateway timeout on timeout", result4.StatusCode.Equals(HttpStatusCode.GatewayTimeout));
        }

        public static void AssertTrue(string name, Boolean b)
        {
            if (b)
            {
                Console.WriteLine($"OK: {name}");
            }
            else
            {
                Console.WriteLine($"Failed: {name}");
            }
        }

        public static void Assert(string name, string expected, string current)
        {
            AssertTrue(name, expected.Equals(current));
        }

        static async Task<LoadBalancer.LoadBalancer> SetupLoadBalancer(LoadBalancer.Configuration settings) {
            var loadBalancer = new LoadBalancer.LoadBalancer(settings);

            await loadBalancer.Start();

            return loadBalancer;
        }

        static WebServer SetupHttpServer(int port, Func<HttpListenerContext, string> request)
        {
            WebServer ws = new WebServer(request, $"http://localhost:{port}/");

            ws.Run();

            return ws;
        }
    }
}
