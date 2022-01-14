using System;
using System.Collections.Generic;

namespace Minio.Client.Http.Internal
{
    internal class MinioLimitation
    {
        /// <summary>
        /// 5Mb
        /// </summary>
        internal const long MinPartSize = 5 * 1024 * 1024;

        /// <summary>
        /// 5Gb
        /// </summary>
        internal const long MaxPartSize = 5L * 1024L * 1024L * 1024L;

        internal const int MaxPartCount = 10000;

        internal const int MaxObjectNameLenght = 1024;

        internal const int MaxObjectSegmentLenght = 255;

        internal const int MaxBucketNameLenght = 255;

        internal const long MaxObjectCopySize = 1024L * 1024L * 1024L * 5;

        internal static readonly HashSet<string> SupportedHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "cache-control",
            "content-encoding",
            "content-type",
            "x-amz-acl",
            "content-disposition"
        };


        internal static long GetPartSize(long value)
        {
            return Math.Min(Math.Max(MinPartSize, value), MaxPartSize);
        }

    }
}