using System;
using System.Text;

namespace Minio.Client.Http.Internal
{
    public readonly struct ObjectIdentifire
    {
        public ObjectIdentifire(string bucketName, string objectName = null)
        {
            var sb = new StringBuilder();
            sb.Append(Uri.EscapeDataString(bucketName));

            if (string.IsNullOrEmpty(objectName))
            {
                sb.Append('/');
            }
            else
            {
                var q = objectName.AsSpan();
                while (!q.IsEmpty)
                {
                    ReadOnlySpan<char> segment;
                    var delimeterIndex = q.IndexOf('/');

                    if (delimeterIndex >= 0)
                    {
                        segment = q.Slice(0, delimeterIndex);
                        q = q.Slice(delimeterIndex + 1);

                    }
                    else
                    {
                        segment = q;
                        q = default;
                    }
                    if (!segment.IsEmpty)
                    {
                        sb.Append('/').Append(Uri.EscapeDataString(segment.ToString()));
                    }
                }
            }

            Path = sb.ToString();
        }

        public string Path { get; }

        public override string ToString()
        {
            return Path;
        }
    }
}