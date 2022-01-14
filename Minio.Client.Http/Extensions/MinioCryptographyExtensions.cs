using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
#if NET5_0_OR_GREATER
using System.Threading;
using System.Threading.Tasks;
#endif
namespace Minio.Client.Http.Extensions
{
    public static class MinioCryptographyExtensions
    {
        private static readonly byte[] _serviceBytes = Encoding.UTF8.GetBytes("s3");

        private static readonly byte[] _requestBytes = Encoding.UTF8.GetBytes("aws4_request");

        /// <summary>
        /// for 'Content-MD5' http header
        /// </summary>
        /// <param name="stream">request body</param>
        /// <returns></returns>
        public static string GetMD5(this Stream stream)
        {
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(stream);
            return Convert.ToBase64String(hash);
        }

#if NET5_0_OR_GREATER
        public static async Task<string> GetMD5Async(this Stream stream, CancellationToken cancellationToken = default)
        {
            using var md5 = MD5.Create();
            var hash = await md5.ComputeHashAsync(stream, cancellationToken);
            return Convert.ToBase64String(hash);
        }
#endif
        /// <summary>
        /// for 'x-amz-content-sha256' http header
        /// <para>
        ///  Note: No need to compute SHA256 if endpoint scheme is https
        /// </para>
        /// </summary>
        /// <param name="stream">request body</param>
        /// <returns></returns>
        public static string GetSHA256(this Stream stream)
        {
            using var md5 = SHA256.Create();
            var hash = md5.ComputeHash(stream);
            return hash.GetHex();
        }

#if NET5_0_OR_GREATER
        public static async Task<string> GetSHA256Async(this Stream stream, CancellationToken cancellationToken = default)
        {
            using var md5 = SHA256.Create();

            var hash = await md5.ComputeHashAsync(stream, cancellationToken).ConfigureAwait(false);
            return hash.GetHex();
        }
#endif
        public static string GetSHA256(this StringBuilder sb)
        {
#if NET5_0_OR_GREATER
            var input = sb.ToString();

            using var owner = System.Buffers.MemoryPool<byte>.Shared.Rent(input.Length);

            var count = Encoding.UTF8.GetBytes(input.AsSpan(), owner.Memory.Span);

            using var sha = SHA256.Create();

            sha.TryComputeHash(owner.Memory.Span[..count], owner.Memory.Span, out count);

            return Convert.ToHexString(owner.Memory.Span[..count]).ToLower();
#else
            using var md5 = SHA256.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
            return hash.GetHex();
#endif

        }

        public static string GetHex(this byte[] bytes)
        {
#if NET5_0_OR_GREATER
            return Convert.ToHexString(bytes).ToLower();
#else
            return BitConverter.ToString(bytes).Replace("-", string.Empty).ToLower();
#endif
        }

        public static byte[] GetHMAC(this byte[] key, byte[] content)
        {
            using HMACSHA256 hmac = new HMACSHA256(key);
            return hmac.ComputeHash(content);
        }

        public static string GetSignature(this byte[] signedContent
           , DateTime signingDate
           , MinioOptions options
           )
        {
            byte[] formattedDateBytes = Encoding.UTF8.GetBytes(signingDate.ToString("yyyMMdd"));

            return options.SecretKeyBytes
                .GetHMAC(formattedDateBytes)
                .GetHMAC(options.RegionBytes)
                .GetHMAC(_serviceBytes)
                .GetHMAC(_requestBytes)
                .GetHMAC(signedContent)
                .GetHex();
        }
    }
}