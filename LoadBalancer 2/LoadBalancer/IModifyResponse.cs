using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadBalancer
{
    public interface IRegisterAndModifyResponse
    {
        Http.Response RegisterAndModifyResponse(Server server, Http.Request request, Http.Response response);
    }
}
