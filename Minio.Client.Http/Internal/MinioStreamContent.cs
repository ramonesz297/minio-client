using System.IO;
using System.Net.Http;

namespace Minio.Client.Http.Internal
{
    internal class MinioStreamContent : StreamContent
    {
        private readonly Stream _content;
        private readonly bool _leaveOpen;

        internal MinioStreamContent(Stream content, bool leaveOpen = false) : base(content)
        {
            _content = content;
            _leaveOpen = leaveOpen;
        }

        internal Stream UnderlingStream => _content;

        protected override void Dispose(bool disposing)
        {
            if (!_leaveOpen)
            {
                base.Dispose(disposing);
            }
        }
    }
}