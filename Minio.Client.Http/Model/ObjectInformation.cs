using System;
using System.Collections.Generic;

namespace Minio.Client.Http.Model
{
    public readonly struct ObjectInformation
    {
        public ObjectInformation(string objectName,
                                 long size,
                                 DateTimeOffset? lastModified,
                                 string eTag,
                                 string contentType,
                                 Dictionary<string, IEnumerable<string>> metadata = null)
        {
            ObjectName = objectName;
            Size = size;
            LastModified = lastModified;
            ETag = eTag;
            ContentType = contentType;
            Metadata = metadata;
        }

        public string ObjectName { get; }

        public long Size { get; }

        public DateTimeOffset? LastModified { get; }

        public string ETag { get; }

        public string ContentType { get; }

        public Dictionary<string, IEnumerable<string>> Metadata { get; }

    }
}
