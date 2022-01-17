using System.Net;
using System.Text;

namespace Minio.Client.Http
{
    public readonly struct GetObjectsRequests
    {
        public GetObjectsRequests(string bucket, bool recursive = false, string prefix = "", int maxKeys = 100, string marker = "")
        {
            Bucket = bucket;
            Recursive = recursive;
            Prefix = prefix;
            MaxKeys = maxKeys;
            Marker = marker;
        }

        public string Bucket { get; }

        public bool Recursive { get; }

        public string Prefix { get; }

        public int MaxKeys { get; }

        public string Marker { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(Bucket).Append('?');

            if (Recursive)
            {
                sb.Append("delimiter=").Append('&');
            }
            else
            {
                sb.Append("delimiter=").Append('/').Append('&');
            }

            sb.Append("prefix=").Append(Prefix).Append('&');

            sb.Append("max-keys=").Append(MaxKeys).Append('&');

            sb.Append("marker=").Append(WebUtility.UrlDecode(Marker)).Append('&');

            sb.Append("encoding-type=url");

            return sb.ToString();
        }

    }
}