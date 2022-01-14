using System.Collections.Generic;

namespace Minio.Client.Http
{
    public readonly struct CompleteMultipartUploadRequest
    {
        public CompleteMultipartUploadRequest(string uploadId)
        {
            UploadId = uploadId;
            ETags = new Dictionary<int, string>();
        }

        public string UploadId { get; }

        public Dictionary<int, string> ETags { get; }

        public void Add(int key, string value)
        {
            ETags.Add(key, value);
        }
    }
}