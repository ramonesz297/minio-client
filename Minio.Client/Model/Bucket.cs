using System;

namespace Minio.Client.Model
{

    [Serializable]
    public class Bucket
    {
        public string Name { get; set; }
        public DateTimeOffset CreationDate { get; set; }
    }
}
