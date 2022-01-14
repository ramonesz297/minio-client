using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Minio.Client.Http.Model
{
    [Serializable]
    [XmlRoot(ElementName = "ListAllMyBucketsResult", Namespace = "http://s3.amazonaws.com/doc/2006-03-01/")]
    [XmlInclude(typeof(Bucket))]
    public class ListAllMyBucketsResult
    {
        [XmlElement(ElementName = "Owner")]
        public Owner Owner { get; set; }

        [XmlArray("Buckets")]
        [XmlArrayItem(typeof(Bucket))]
        public List<Bucket> Buckets { get; set; }
    }
}
