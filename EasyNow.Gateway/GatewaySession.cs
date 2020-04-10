using System;
using System.Net;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using SuperSocket.Client;
using SuperSocket.Server;

namespace EasyNow.Gateway
{
    public class GatewaySession:AppSession
    {
        public ILifetimeScope LifetimeScope { get; }

        public GatewaySession(ILifetimeScope lifetimeScope)
        {
            this.LifetimeScope = lifetimeScope;
        }

        protected override async ValueTask OnSessionConnectedAsync()
        {
            await base.OnSessionConnectedAsync();
        }

        protected override async ValueTask OnSessionClosedAsync(EventArgs e)
        {
            await base.OnSessionClosedAsync(e);
        }

        public override ValueTask CloseAsync()
        {
            LifetimeScope.Dispose();
            return base.CloseAsync();
        }
    }
}