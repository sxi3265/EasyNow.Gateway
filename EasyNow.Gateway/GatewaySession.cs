using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SuperSocket.Client;
using SuperSocket.Server;

namespace EasyNow.Gateway
{
    public class GatewaySession:AppSession
    {
        //public EasyClient<byte[]> Client { get; private set; }

        protected override async ValueTask OnSessionConnectedAsync()
        {
            //this.Client = new EasyClient<byte[]>(new BufferPipelineFilter());
            await base.OnSessionConnectedAsync();
        }

        protected override async ValueTask OnSessionClosedAsync(EventArgs e)
        {
            await base.OnSessionClosedAsync(e);
        }
    }
}