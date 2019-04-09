using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadBalancer
{
    public class Server
    {
        public String Endpoint { get; set; }

        public int Port { get; set; }

        public override string ToString()
        {
            return $"{Endpoint}:{Port}" ;
        }
    }
}
