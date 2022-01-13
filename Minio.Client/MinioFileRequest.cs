using Minio.Client.Internal;
using System.Collections.Generic;
using System.IO;

namespace Minio.Client
{

    public readonly struct MinioFileRequest
    {
        public MinioFileRequest(Stream stream,
            string contentType = "application/octet-stream",
            Dictionary<string, string> metadata = null,
            string uploadId = null,
            int? partId = null,
            long? partSize = null,
            long? maxSingleSizeUpload = null)
        {
            Stream = stream;
            ContentType = contentType;
            Metadata = metadata;
            UploadId = uploadId;
            PartId = partId;
            PartSize = partSize.HasValue ? MinioLimitation.GetPartSize(partSize.Value) : (long?)null;
            MaxSingleSizeUpload = maxSingleSizeUpload.HasValue ? MinioLimitation.GetPartSize(maxSingleSizeUpload.Value) : (long?)null;
        }

        public Stream Stream { get; }

        public string ContentType { get; }

        public Dictionary<string, string> Metadata { get; }

        public string UploadId { get; }

        public int? PartId { get; }

        public long? PartSize { get; }

        public long? MaxSingleSizeUpload { get; }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(UploadId) || !PartId.HasValue)
            {
                return "";
            }

            return $"?uploadId={UploadId}&partNumber={PartId}";
        }
    }
}