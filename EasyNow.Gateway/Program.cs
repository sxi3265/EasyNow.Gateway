using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using DnsClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SuperSocket;
using SuperSocket.Client;

namespace EasyNow.Gateway
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var stack = new ConcurrentStack<IEasyClient<HttpResponse>>();
            var lookupClient = new LookupClient(new LookupClientOptions
            {
                UseCache = true
            });
            var host = SuperSocketHostBuilder
                .Create<HttpRequest, GatewayService, HttpPipelineFilter>()
                .ConfigurePackageHandler<HttpRequest>(async (s, p) =>
                {
                    var appSession = (GatewaySession) s;
                    var options = appSession.LifetimeScope.Resolve<IOptionsSnapshot<GatewayOptions>>();
                    var forwardConfig = options.Value.ForwardConfigs.FirstOrDefault(e => e.Path == p.Path);
                    if (forwardConfig == null)
                    {
                        //todo 返回404
                        return;
                    }
                    p.Header["host"] = forwardConfig.Target.Host;
                    var request = p.ToString();
                    if (!stack.TryPop(out var client))
                    {
                        //client= new EasyClient<HttpResponse>(new BufferPipelineFilter());
                        client = new SecureClient<HttpResponse>(new BufferPipelineFilter()).AsClient();
                    }
                    var result = await lookupClient.QueryAsync(forwardConfig.Target.Host, QueryType.A);
                    var record = result.Answers.ARecords().FirstOrDefault();
                    await client.ConnectAsync(new IPEndPoint(record.Address, forwardConfig.Target.Port));
                    await client.SendAsync(new DefaultStringEncoder(), request);
                    var response = await client.ReceiveAsync();
                    await client.CloseAsync();
                    stack.Push(client);
                    if (response != null)
                    {
                        await s.SendAsync(new HttpResponsePackageEncoder(), response);
                    }
                }, (s, e) => { return new ValueTask<bool>(); })
                .ConfigureLogging((hostCtx, loggingBuilder) => { loggingBuilder.AddConsole(); })
                .ConfigureServices((hostCtx, services) =>
                {
                    services.Configure<GatewayOptions>(hostCtx.Configuration.GetSection("Gateway"));
                })
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>(builder =>
                {
                    builder.RegisterType<GatewaySessionFactory>().AsImplementedInterfaces().SingleInstance();
                    builder.RegisterType<GatewaySession>().InstancePerLifetimeScope();
                })
                .Build();
            await host.RunAsync();
        }
    }
}
