using System;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace Minio.Client.Model
{

    [Serializable]
    public class Bucket
    {
        public string Name { get; set; }
        public DateTimeOffset CreationDate { get; set; }
    }
}
