using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Minio.Client.Http.Internal
{
    internal sealed class PartialStream : Stream
    {
        private readonly Stream _stream;
        private readonly long _partSize;
        private readonly bool _leaveOpen;
        private long _length;

        public override bool CanRead => _stream.CanRead;
        public override bool CanSeek => _stream.CanSeek;
        public override bool CanWrite => false;
        public override long Length => _length;
        public override long Position { get => _stream.Position; set => _stream.Position = value; }

        public long OriginalLength { get; }

        public bool ShouldMultipart => OriginalLength > _partSize;

        public int CurrentPart { get; private set; } = 1;

        internal PartialStream(Stream stream, bool leaveOpen, long? partSize = null)
        {
            _length = 0;
            var partSizer = new PartSizer(stream.Length, partSize);
            OriginalLength = stream.Length;
            _stream = stream;
            _partSize = partSizer.PartSize;
            _leaveOpen = leaveOpen;
            SetLength(Math.Min(partSizer.PartSize, stream.Length));
        }

        public bool TryExtend()
        {
            if (OriginalLength > Length)
            {
                SetLength(Math.Min(OriginalLength, _stream.Position + _partSize));
                CurrentPart++;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void Flush()
        {
            _stream.Flush();
        }

        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            await _stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (Position == Length)
            {
                return 0;
            }

            if (Position + count > Length)
            {
                count = (int)(Length - Position);
            }

            return _stream.Read(buffer, offset, count);
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (Position == Length)
            {
                return 0;
            }

            if (Position + count > Length)
            {
                count = (int)(Length - Position);
            }

            return await _stream.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
        }

#if NETCOREAPP3_1_OR_GREATER
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (Position == Length)
            {
                return 0;
            }

            if (Position + buffer.Length > Length)
            {
                var count = (int)(Length - Position);

                var limitedBuffer = buffer[0..count];

                count = await _stream.ReadAsync(limitedBuffer, cancellationToken).ConfigureAwait(false);

                return count;
            }
            else
            {
                return await _stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            }
        }
#endif

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (ShouldMultipart && origin == SeekOrigin.Begin)
            {
                return _stream.Seek(offset + (CurrentPart - 1) * _partSize, origin);
            }
            else
            {
                return _stream.Seek(offset, origin);
            }
        }

        public override void SetLength(long value)
        {
            _length = value;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing && !_leaveOpen)
            {
                _stream.Dispose();
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        public override async ValueTask DisposeAsync()
        {
            if (!_leaveOpen)
            {
                await _stream.DisposeAsync().ConfigureAwait(false);
            }
        }
#endif
    }
}