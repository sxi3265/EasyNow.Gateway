using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SuperSocket;
using SuperSocket.Server;

namespace EasyNow.Gateway
{
    public class GatewayService:SuperSocketService<HttpRequest>
    {
        public GatewayService(IServiceProvider serviceProvider, IOptions<ServerOptions> serverOptions,
            ILoggerFactory loggerFactory, IChannelCreatorFactory channelCreatorFactory) : base(serviceProvider,
            serverOptions, loggerFactory, channelCreatorFactory)
        {
        }
    }
}