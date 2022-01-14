using Microsoft.Extensions.Primitives;
using Minio.Client.Http.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Minio.Client.Http.Extensions
{

    public static class MinioHttpRequestExtensions
    {
        private const string _sha256EmptyFileHash = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";

        private static readonly HashSet<string> _ignoredHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "authorization",
            "content-length",
            "content-type",
            "user-agent"
        };

        public static bool IsEmptyRequest(this HttpRequestMessage request)
        {
#if NET5_0_OR_GREATER
            request.Options.TryGetValue(new HttpRequestOptionsKey<bool>("is_empty"), out bool result);
            return result;
#else
            return request.Properties.ContainsKey("is_empty");
#endif
        }

        public static HttpRequestMessage SetEmptyRequest(this HttpRequestMessage request)
        {

#if NET5_0_OR_GREATER
            request.Options.Set(new HttpRequestOptionsKey<bool>("is_empty"), true);
#else
            request.Properties.Add("is_empty", true);
#endif
            return request;
        }

        public static bool IsSecure(this HttpRequestMessage request)
        {
            return string.Equals(request.RequestUri.Scheme, "https", StringComparison.OrdinalIgnoreCase);
        }
        private static async Task<Stream> ReadAsStreamAsync(this HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            return request.Content switch
            {
                MinioStreamContent content => content.UnderlyingStream,
#if NET5_0_OR_GREATER
                _ => await request.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false)
#else
                _ => await request.Content.ReadAsStreamAsync().ConfigureAwait(false)
#endif
            };
        }

        public static async Task<HttpRequestMessage> AddMD5HeaderAsync(this HttpRequestMessage request, MinioOptions options, CancellationToken cancellationToken = default)
        {
            if (request.Method != HttpMethod.Post && request.Method != HttpMethod.Put)
            {
                return request;
            }

            if (!request.IsSecure() && !options.IsAnonymous && request.RequestUri.Query.IndexOf("delete", StringComparison.OrdinalIgnoreCase) == -1)
            {
                return request;
            }

            Stream requestBody = await request.ReadAsStreamAsync(cancellationToken);

#if NET5_0_OR_GREATER
            var hash = await requestBody.GetMD5Async(cancellationToken).ConfigureAwait(false);
#else

            var hash = requestBody.GetMD5();
#endif
            requestBody.Seek(0, SeekOrigin.Begin);

            request.Headers.TryAddWithoutValidation("Content-MD5", hash);

            return request;
        }


        public static async Task<HttpRequestMessage> AddSHA256HeaderAsync(this HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            if (request.IsSecure())
            {
                request.Headers.TryAddWithoutValidation("x-amz-content-sha256", "UNSIGNED-PAYLOAD");
                return request;
            }

            if ((request.Method == HttpMethod.Post || request.Method == HttpMethod.Put) && !request.IsEmptyRequest())
            {
                Stream requestBody = await request.ReadAsStreamAsync(cancellationToken);

#if NET5_0_OR_GREATER
                var hash = await requestBody.GetSHA256Async(cancellationToken).ConfigureAwait(false);
#else
                var hash = requestBody.GetSHA256();
#endif
                requestBody.Seek(0, SeekOrigin.Begin);

                request.Headers.TryAddWithoutValidation("x-amz-content-sha256", hash);

                return request;
            }
            else
            {
                request.Headers.TryAddWithoutValidation("x-amz-content-sha256", _sha256EmptyFileHash);

                return request;
            }
        }

        public static HttpRequestMessage AddHostHeader(this HttpRequestMessage request)
        {
            var url = $"{request.RequestUri.Host}:{request.RequestUri.Port}";
            request.Headers.TryAddWithoutValidation("Host", url);
            return request;
        }

        public static HttpRequestMessage AddDateHeader(this HttpRequestMessage request, DateTime signingDate)
        {
            request.Headers.TryAddWithoutValidation("x-amz-date", signingDate.ToString("yyyyMMddTHHmmssZ"));
            return request;
        }

        public static HttpRequestMessage AddSessionTokenHeader(this HttpRequestMessage request, string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return request;
            }

            request.Headers.TryAddWithoutValidation("X-Amz-Security-Token", token);

            return request;
        }

        public static SortedDictionary<string, StringValues> GetSigningHeaders(this HttpRequestMessage request)
        {
            var headers = request.Headers.Where(x => !_ignoredHeaders.Contains(x.Key))
             .ToDictionary(s => s.Key.ToLower(), s => new StringValues(s.Value.ToArray()), StringComparer.OrdinalIgnoreCase);

            return new SortedDictionary<string, StringValues>(headers, StringComparer.Ordinal);
        }

        internal static QueryStringCollection GetQuery(this HttpRequestMessage request, bool escape = true)
        {
            return request.RequestUri.GetQuery(escape);
        }

        internal static QueryStringCollection GetQuery(this Uri request, bool escape = true)
        {
            return new QueryStringCollection(request.Query, escape);
        }

        private static string GetScope(string region, DateTime signingDate)
        {
            return $"{signingDate:yyyyMMdd}/{region}/s3/aws4_request";
        }

        public static byte[] GetPresignedContent(this Uri request, string query, HttpMethod httpMethod, DateTime signingDate, MinioOptions options)
        {
            var sb = new StringBuilder();

            sb.Append(httpMethod).Append('\n');
            sb.Append(request.AbsolutePath).Append('\n');

            sb.Append(query).Append('\n');

            sb.Append("host:").Append(request.Authority).Append('\n');
            sb.Append('\n');
            sb.Append("host").Append('\n');
            sb.Append("UNSIGNED-PAYLOAD");

            var scope = GetScope(options.Region, signingDate);

            return Encoding.UTF8.GetBytes($"AWS4-HMAC-SHA256\n{signingDate:yyyyMMddTHHmmssZ}\n{scope}\n{sb.GetSHA256()}");

        }


        public static byte[] GetSignedContent(this HttpRequestMessage request
            , DateTime signingDate
            , SortedDictionary<string, StringValues> headersToSign
            , MinioOptions options)
        {

            var sb = new StringBuilder();

            sb.Append(request.Method).Append('\n');
            sb.Append(request.RequestUri.LocalPath);
            sb.Append('\n');
            sb.Append(request.GetQuery().ToString());
            sb.Append('\n');

            foreach (var item in headersToSign)
            {
                sb.Append(item.Key).Append(':').Append(item.Value).Append('\n');
            }

            sb.Append('\n');

#if NETCOREAPP3_1_OR_GREATER
            sb.AppendJoin(';', headersToSign.Keys).Append('\n');
#else
            sb.Append(string.Join(";", headersToSign.Keys)).Append('\n');
#endif

            if (headersToSign.TryGetValue("x-amz-content-sha256", out var header))
            {
                sb.Append(header);
            }
            else
            {
                sb.Append(_sha256EmptyFileHash);
            }

            var scope = GetScope(options.Region, signingDate);

            return Encoding.UTF8.GetBytes($"AWS4-HMAC-SHA256\n{signingDate:yyyyMMddTHHmmssZ}\n{scope}\n{sb.GetSHA256()}");
        }

        public static Uri PresignUrl(this Uri uri, HttpMethod httpMethod
            , int expires, MinioOptions options
            , SortedDictionary<string, StringValues> headers = null)
        {
            var now = DateTime.UtcNow;
            var query = uri.GetQuery(true);

            query.Add("X-Amz-Algorithm", "AWS4-HMAC-SHA256");
            query.Add("X-Amz-Credential", options.AccessKey + "/" + GetScope(options.Region, now));
            query.Add("X-Amz-Date", now.ToString("yyyyMMddTHHmmssZ"));
            query.Add("X-Amz-Expires", expires.ToString());
            query.Add("X-Amz-SignedHeaders", "host");

            if (headers != null && headers.Count > 0)
            {
                foreach (var item in headers)
                {
                    query.Add(item.Key, item.Value);
                }
            }
            if (!string.IsNullOrEmpty(options.SessionToken))
            {
                query.Add("X-Amz-Security-Token", options.SessionToken);
            }

            var unsignedQuery = query.ToString();

            var signature = uri.GetPresignedContent(unsignedQuery, httpMethod, now, options).GetSignature(now, options);

            return new Uri(uri, $"?{unsignedQuery}&X-Amz-Signature={signature}");
        }

        public static HttpRequestMessage PresignUrl(this HttpRequestMessage request, int expires, MinioOptions options)
        {
            request.RequestUri = request.RequestUri.PresignUrl(request.Method, expires, options, request.GetSigningHeaders());

            return request;
        }

        public static async Task AddAccessToken(this HttpRequestMessage request, MinioOptions options, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            await request.AddMD5HeaderAsync(options, cancellationToken).ConfigureAwait(false);
            await request.AddSHA256HeaderAsync(cancellationToken).ConfigureAwait(false);
            request.AddHostHeader();
            request.AddDateHeader(now);
            request.AddSessionTokenHeader(options.SessionToken);

            var header = request.GetSigningHeaders();

            var signature = request.GetSignedContent(now, header, options).GetSignature(now, options);

            var signedHeaders = string.Join(";", header.Keys);

            var accessToken = $"AWS4-HMAC-SHA256 Credential={options.AccessKey}/{GetScope(options.Region, now)}, SignedHeaders={signedHeaders}, Signature={signature}";

            request.Headers.TryAddWithoutValidation("Authorization", accessToken);
        }

    }
}