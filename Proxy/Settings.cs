using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proxy
{
    public class Settings
    {
        public bool CachingEnabled { get; set; } = false;

        public bool AuthenticationEnabled { get; set; } = false;

        public bool PrivacyModusEnabled { get; set; } = false;

        public bool AdBlockerEnabled { get; set; } = false;

        public int BufferSize { get; set; } = 1024;

        public int Port { get; set; } = 8080;

        public bool TestMode { get; set; } = true;
    }
}
