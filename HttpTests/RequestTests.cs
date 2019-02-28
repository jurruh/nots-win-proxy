using Microsoft.VisualStudio.TestTools.UnitTesting;
using Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http.Tests
{
    [TestClass()]
    public class RequestTests
    {
        [TestMethod()]
        public void RequestTest()
        {
            Request request = new Request("GET /pub/WWW/TheProject.html HTTP/1.1\r\nHost: www.w3.org\r\nAccept: application/json");

            Assert.AreEqual(" www.w3.org", request.Headers["host"]);
            Assert.AreEqual(" application/json", request.Headers["accept"]);
        }

        [TestMethod()]
        public void ResolveToAddressTest()
        {
            Request request = new Request("GET http://jurrevriesen.nl/ HTTP/1.1\r\nAccept: text/html, application/xhtml+xml, image/jxr, */*\r\n\r\n");
            Assert.AreEqual("http://jurrevriesen.nl/", request.ResolveToAddress());
        }
    }
}