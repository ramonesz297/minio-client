using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Minio.Client.Http.Model
{
    [Serializable]
    public class Owner
    {
        [XmlElement("ID")]
        public string Id { get; set; }

        [XmlElement("DisplayName")]
        public string DisplayName { get; set; }
    }

    [Serializable]
    [XmlRoot(ElementName = "Contents", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
    public class Contents
    {
        [XmlElement(ElementName = "Key")]
        public string Key { get; set; }

        [XmlElement(ElementName = "LastModified")]
        public DateTime LastModified { get; set; }

        [XmlElement(ElementName = "ETag")]
        public string ETag { get; set; }

        [XmlElement(ElementName = "Size")]
        public long Size { get; set; }

        [XmlElement(ElementName = "Owner")]
        public Owner Owner { get; set; }

        [XmlElement(ElementName = "StorageClass")]
        public string StorageClass { get; set; }
    }

    [Serializable]
    [XmlRoot(ElementName = "ListBucketResult", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
    public class ListBucketResult
    {
        [XmlElement(ElementName = "Name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "Prefix")]
        public string Prefix { get; set; }

        [XmlElement(ElementName = "Marker")]
        public string Marker { get; set; }

        [XmlElement(ElementName = "NextMarker")]
        public string NextMarker { get; set; }

        [XmlElement(ElementName = "VersionIdMarker")]
        public string VersionIdMarker { get; set; }

        [XmlElement(ElementName = "NextContinuationToken")]
        public string NextContinuationToken { get; set; }

        [XmlElement(ElementName = "MaxKeys")]
        public int MaxKeys { get; set; }

        [XmlElement(ElementName = "KeyCount")]
        public int? KeyCount { get; set; }

        [XmlElement(ElementName = "Delimiter")]
        public string Delimiter { get; set; }

        [XmlElement(ElementName = "IsTruncated")]
        public bool IsTruncated { get; set; }

        [XmlElement(ElementName = "Contents")]
        public List<Contents> Contents { get; set; }

        [XmlElement(ElementName = "EncodingType")]
        public string EncodingType { get; set; }

    }

}


