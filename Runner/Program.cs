﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runner
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var proxy = new Proxy.Proxy(new Proxy.Configuration());
            proxy.Start();
            //Console.ReadLine();
        }
    }
}
