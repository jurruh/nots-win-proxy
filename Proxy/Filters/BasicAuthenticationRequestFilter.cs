using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Http;

namespace Proxy.Filters
{
    public class BasicAuthenticationRequestFilter : Filter<Http.Request>
    {
        const string USERNAME = "admin";
        const string PASSWORD = "admin";

        public override Request Apply(Request message)
        {
            if (message.Headers.ContainsKey("Proxy-authorization") &&
                message.Headers["Proxy-authorization"].Contains("Basic "))
            {
                var base64 = message.Headers["Proxy-authorization"].Replace("Basic ", "");
                var decoded = Base64Decode(base64);

                if (decoded.Equals($"{USERNAME}:{PASSWORD}"))
                {
                    return message;
                }

            }

            abort();
            return null;
        }

        private string Base64Decode(string encodedString)
        {
            byte[] data = Convert.FromBase64String(encodedString);
            return Encoding.UTF8.GetString(data);
        }

        private void abort()
        {
            this.AbortResponse = new Http.Response();
            this.AbortResponse.Status = 407;
            this.AbortResponse.StatusMessage = "Proxy Authentication Required";
            //this.AbortResponse.Body = Encoding.ASCII.GetBytes("Proxy Authentication Required"); Todo fix
        }
    }
}
