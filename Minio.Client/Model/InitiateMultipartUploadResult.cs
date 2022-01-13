﻿using System;
using System.Xml.Serialization;

namespace Minio.Client.Model
{
    [Serializable]
    [XmlRoot(ElementName = "InitiateMultipartUploadResult", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
    public class InitiateMultipartUploadResult
    {
        public string UploadId { get; set; }
    }

    [Serializable]
    [XmlRoot(ElementName = "CopyPartResult", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
    public class CopyPartResult
    {
        public DateTimeOffset LastModified { get; set; }

        public string ETag { get; set; }
    }
}
