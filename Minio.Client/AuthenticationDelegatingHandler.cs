using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Minio.Client
{
    public sealed class AuthenticationDelegatingHandler : DelegatingHandler
    {
        private readonly IOptions<MinioOptions> _options;

        public AuthenticationDelegatingHandler(IOptions<MinioOptions> options)
        {
            _options = options;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await request.AddAccessToken(_options.Value).ConfigureAwait(false);

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

    }
}