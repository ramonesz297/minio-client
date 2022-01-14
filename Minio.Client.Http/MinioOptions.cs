using Minio.Client.Http.Internal;
using System.Text;

namespace Minio.Client.Http
{
    public class MinioOptions
    {
        private string _region = "us-east-1";
        private string _secretKey;
        private long _maxSingleSizeUpload = MinioLimitation.MinPartSize;
        private long? _defaultMultipartSize;

        public string AccessKey { get; set; }

        public string SessionToken { get; set; }

        public string SecretKey
        {
            get => _secretKey;
            set
            {
                _secretKey = value;
                SecretKeyBytes = Encoding.UTF8.GetBytes($"AWS4{_secretKey}");

            }
        }

        public string Region
        {
            get => _region;
            set
            {
                _region = value;
                RegionBytes = Encoding.UTF8.GetBytes(_region);
            }
        }

        public long MaxSingleSizeUpload { get => _maxSingleSizeUpload; set => _maxSingleSizeUpload = MinioLimitation.GetPartSize(value); }

        public long? DefaultMultipartSize
        {
            get => _defaultMultipartSize; set
            {
                if (value.HasValue)
                {
                    _defaultMultipartSize = MinioLimitation.GetPartSize(value.Value);
                }
                else
                {
                    _defaultMultipartSize = value;
                }
            }
        }

        public bool IsAnonymous => string.IsNullOrEmpty(SecretKey) && string.IsNullOrEmpty(AccessKey);

        internal byte[] RegionBytes { get; private set; } = new byte[] { 117, 115, 45, 101, 97, 115, 116, 45, 49 };

        internal byte[] SecretKeyBytes { get; private set; }

    }
}