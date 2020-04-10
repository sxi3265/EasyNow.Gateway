using System;
using Autofac;
using SuperSocket;

namespace EasyNow.Gateway
{
    public class GatewaySessionFactory:ISessionFactory
    {
        private readonly ILifetimeScope _lifetimeScope;

        public GatewaySessionFactory(ILifetimeScope lifetimeScope)
        {
            this._lifetimeScope = lifetimeScope;
        }

        public IAppSession Create()
        {
            return _lifetimeScope.BeginLifetimeScope().Resolve<GatewaySession>();
        }

        public Type SessionType => typeof(GatewaySession);
    }
}