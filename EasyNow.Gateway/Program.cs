using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket;
using SuperSocket.Client;

namespace EasyNow.Gateway
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var stack = new ConcurrentStack<EasyClient<HttpResponse>>();
            var host = SuperSocketHostBuilder
                .Create<HttpRequest,GatewayService, HttpPipelineFilter>()
                .ConfigurePackageHandler(async (s, p) =>
                {
                    p.Header["host"]="www.sjqgw.com";
                    p.Header["Accept-Encoding"] = string.Empty;
                    var request = p.ToString();
                    if (!stack.TryPop(out var client))
                    {
                        client= new EasyClient<HttpResponse>(new BufferPipelineFilter());
                    }
                    await client.ConnectAsync(new IPEndPoint(IPAddress.Parse("150.138.249.207"), 80));
                    await client.SendAsync(new DefaultStringEncoder(),request);
                    var response = await client.ReceiveAsync();
                    await client.CloseAsync();
                    stack.Push(client);
                    await s.SendAsync(new HttpResponsePackageEncoder(),response);
                })
                .ConfigureLogging((hostCtx, loggingBuilder) =>
                {
                    loggingBuilder.AddConsole();
                })
                .UseSession<GatewaySession>()
                .Build();
            await host.RunAsync();
        }
    }
}
