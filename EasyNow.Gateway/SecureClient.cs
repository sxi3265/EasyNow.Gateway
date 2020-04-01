using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using SuperSocket.Client;
using SuperSocket.ProtoBase;

namespace EasyNow.Gateway
{
    public class SecureClient<TReceivePackage> : EasyClient<TReceivePackage> where TReceivePackage : class
    {
        public SecureClient(IPipelineFilter<TReceivePackage> pipelineFilter, ILogger logger = null) : base(pipelineFilter, logger)
        {
        }

        protected override IConnector GetConntector()
        {
            var authOptions = new SslClientAuthenticationOptions
            {
                EnabledSslProtocols = SslProtocols.Tls11 | SslProtocols.Tls12,
                TargetHost = IPAddress.Loopback.ToString()
            };

            // todo 做证书验证
            authOptions.RemoteCertificateValidationCallback += (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;

            return new SocketConnector(new SslStreamConnector(authOptions));
        }
    }
}