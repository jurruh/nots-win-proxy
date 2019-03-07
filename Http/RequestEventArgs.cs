using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http
{
    public class RequestEventArgs : EventArgs
    {
        public Request Request { get; set; }
        public Action<Response> ResponseAction { get; set; }

        public RequestEventArgs(Request request, Action<Response> responseAction)
        {
            this.Request = request;
            this.ResponseAction = responseAction;
        }
    }
}
