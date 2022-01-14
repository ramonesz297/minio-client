using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Minio.Client.Http.Extensions
{
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> HeadAsync(this HttpClient client, string request, CancellationToken cancellationToken = default)
        {
            using var r = new HttpRequestMessage(HttpMethod.Head, request);
            return await client.SendAsync(r, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> PostWithEmptyBodyAsync(this HttpClient client, string request, CancellationToken cancellationToken = default)
        {
            using var r = new HttpRequestMessage(HttpMethod.Post, request)
            {
                Content = new StringContent("", System.Text.Encoding.UTF8),
            };
            return await client.SendAsync(r.SetEmptyRequest(), cancellationToken);

        }

        public static async Task<HttpResponseMessage> PutWithEmptyBodyAsync(this HttpClient client, string request, CancellationToken cancellationToken = default)
        {
            using var r = new HttpRequestMessage(HttpMethod.Put, request)
            {
                Content = new StringContent("", System.Text.Encoding.UTF8),
            };

            return await client.SendAsync(r.SetEmptyRequest(), cancellationToken);

        }
    }
}