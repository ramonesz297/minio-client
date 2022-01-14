using Minio.Client.Http.Internal;
using System;
using System.Collections.Generic;

namespace Minio.Client.Http
{
    public readonly struct CopyRequest
    {
        public CopyRequest(string sourceBucketName,
                           string sourceObjectName,
                           string destinationBucketName,
                           string destinationObjectName = null,
                           CopyReqyuestConditions conditions = default,
                           Dictionary<string, IEnumerable<string>> metadata = null,
                           long? maxSingleSizeUpload = null,
                           long? multipartSize = null)
        {
            SourceBucketName = sourceBucketName;
            SourceObjectName = sourceObjectName;
            DestinationBucketName = destinationBucketName;
            DestinationObjectName = destinationObjectName ?? sourceObjectName;
            Conditions = conditions;
            Metadata = metadata ?? new Dictionary<string, IEnumerable<string>>();
            MaxObjectCopySize = maxSingleSizeUpload.HasValue ? MinioLimitation.GetPartSize(maxSingleSizeUpload.Value) : (long?)null;
            MultipartSize = multipartSize.HasValue ? MinioLimitation.GetPartSize(multipartSize.Value) : (long?)null;
        }

        public string SourceBucketName { get; }

        public string SourceObjectName { get; }

        public string DestinationBucketName { get; }

        public string DestinationObjectName { get; }

        public CopyReqyuestConditions Conditions { get; }

        public Dictionary<string, IEnumerable<string>> Metadata { get; }

        public long? MaxObjectCopySize { get; }

        public long? MultipartSize { get; }

        public bool HasReplaceMetadataDirective()
        {
            return Conditions.Replace;
        }

        public IEnumerable<KeyValuePair<string, IEnumerable<string>>> GetHeaders()
        {
            foreach (var item in Metadata)
            {
                if (!MinioLimitation.SupportedHeaders.Contains(item.Key) && !item.Key.StartsWith("x-amz-meta-", StringComparison.OrdinalIgnoreCase))
                {
                    yield return new KeyValuePair<string, IEnumerable<string>>($"x-amz-meta-{item.Key.ToLowerInvariant()}", item.Value);
                }
                else
                {
                    yield return item;
                }
            }
        }

        public void PopulateHeaders(Dictionary<string, IEnumerable<string>> headers)
        {
            if (!HasReplaceMetadataDirective())
            {
                Metadata.Clear();
                foreach (var item in headers)
                {
                    Metadata.Add(item.Key, item.Value);
                }
            }
        }
    }
}