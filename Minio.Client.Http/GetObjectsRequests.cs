using System.Net;
using System.Text;

namespace Minio.Client.Http
{
    public readonly struct GetObjectsRequests
    {
        public GetObjectsRequests(string bucket,
                                  bool recursive = false,
                                  string prefix = "",
                                  int maxKeys = 100,
                                  string marker = "",
                                  bool useV2 = true,
                                  bool versions = false,
                                  string continuationToken = null,
                                  string versionIdMarker = null)
        {
            Bucket = bucket;
            Recursive = recursive;
            Prefix = prefix;
            MaxKeys = maxKeys;
            Marker = marker;
            UseV2 = useV2;
            Versions = versions;
            ContinuationToken = continuationToken;
            VersionIdMarker = versionIdMarker;
        }

        public string Bucket { get; }

        public bool Recursive { get; }

        public string Prefix { get; }

        public int MaxKeys { get; }

        public string Marker { get; }

        public bool UseV2 { get; }
        public bool Versions { get; }
        public string ContinuationToken { get; }
        public string VersionIdMarker { get; }

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


            if (Versions)
            {
                sb.Append("versions=").Append('&');
                if (!string.IsNullOrEmpty(Marker))
                {
                    sb.Append("key-marker=").Append(Marker).Append('&');
                }
                if (!string.IsNullOrEmpty(VersionIdMarker))
                {
                    sb.Append("version-id-marker=").Append(VersionIdMarker).Append('&');
                }
            }
            else if (!Versions && UseV2)
            {
                sb.Append("list-type=").Append(2).Append('&');
                if (!string.IsNullOrEmpty(Marker))
                {
                    sb.Append("start-after=").Append(Marker).Append('&');
                }
                if (!string.IsNullOrEmpty(ContinuationToken))
                {
                    sb.Append("continuation-token=").Append(ContinuationToken).Append('&');
                }
            }
            else if (!Versions && !UseV2)
            {
                sb.Append("marker=").Append(Marker).Append('&');
            }
            
            sb.Append("encoding-type=url");

            return sb.ToString();
        }

    }
}