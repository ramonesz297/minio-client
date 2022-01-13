using System;
using System.Xml.Serialization;

namespace Minio.Client.Model
{
    [Serializable]
    public class Owner
    {
        [XmlElement("ID")]
        public string Id { get; set; }

        [XmlElement("DisplayName")]
        public string DisplayName { get; set; }
    }
}
