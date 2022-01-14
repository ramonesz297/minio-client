using System;
using System.Collections.Generic;

namespace Minio.Client.Http
{
    public readonly struct CopyReqyuestConditions
    {
        public CopyReqyuestConditions(bool replace = false,
            DateTime? ifModifiedSince = null,
            DateTime? ifUnModifiedSince = null,
            string copySourceIfMatchETag = null,
            string copySourceIfNoneMatchETag = null)
        {
            IfModifiedSince = ifModifiedSince;
            IfUnModifiedSince = ifUnModifiedSince;
            CopySourceIfMatchETag = copySourceIfMatchETag;
            CopySourceIfNoneMatchETag = copySourceIfNoneMatchETag;
            Replace = replace;
        }

        public DateTime? IfModifiedSince { get; }

        public DateTime? IfUnModifiedSince { get; }

        public string CopySourceIfMatchETag { get; }

        public string CopySourceIfNoneMatchETag { get; }

        public bool Replace { get; }

        public IEnumerable<KeyValuePair<string, string>> GetHeaders()
        {
            if (IfModifiedSince.HasValue)
            {
                yield return new KeyValuePair<string, string>("x-amz-copy-source-if-modified-since", IfModifiedSince.Value.ToUniversalTime().ToString("r"));
            }

            if (IfUnModifiedSince.HasValue)
            {
                yield return new KeyValuePair<string, string>("x-amz-copy-source-if-unmodified-since", IfModifiedSince.Value.ToUniversalTime().ToString("r"));
            }

            if (!string.IsNullOrEmpty(CopySourceIfMatchETag))
            {
                yield return new KeyValuePair<string, string>("x-amz-copy-source-if-match", CopySourceIfMatchETag);
            }

            if (!string.IsNullOrEmpty(CopySourceIfNoneMatchETag))
            {
                yield return new KeyValuePair<string, string>("x-amz-copy-source-if-none-match", CopySourceIfNoneMatchETag);
            }

            if (Replace)
            {
                yield return new KeyValuePair<string, string>("x-amz-metadata-directive", "REPLACE");
            }
        }
    }
}