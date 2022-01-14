using System;

namespace Minio.Client.Http.Model
{

    [Serializable]
    public class Bucket
    {
        public string Name { get; set; }
        public DateTimeOffset CreationDate { get; set; }
    }
}
