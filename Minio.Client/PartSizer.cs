using Minio.Client.Internal;
using System;
using System.Collections.Generic;

namespace Minio.Client
{
    public readonly struct PartSizer
    {
        public long PartSize { get; }

        public int PartCount { get; }

        public long LastPartSize { get; }

        public PartSizer(long size, long? partSize = null)
        {
            if (size < MinioLimitation.MinPartSize)
            {
                PartSize = size;
                PartCount = 1;
                LastPartSize = 0;
            }
            else
            {
                if (partSize.HasValue)
                {
                    PartSize = Math.Min(MinioLimitation.MinPartSize, Math.Max(partSize.Value, MinioLimitation.MinPartSize));
                }
                else
                {
                    if (size <= 50 * 1024 * 1024) //50Mb
                    {
                        PartSize = MinioLimitation.MinPartSize;
                    }
                    else if (size <= 500 * 1024 * 1024)//500Mb
                    {
                        PartSize = MinioLimitation.MinPartSize * 4; //20Mb
                    }
                    else if (size <= 5000L * 1024L * 1024L) //5Gb
                    {
                        PartSize = MinioLimitation.MinPartSize * 20; //100Mb
                    }
                    else
                    {
                        PartSize = CalculatePartSize(size);
                    }
                }

                PartCount = (int)Math.Ceiling(size / (double)PartSize);
                LastPartSize = size % PartSize;
            }
        }

        public IEnumerable<Part> GetParts()
        {
            for (int i = 0; i < PartCount; i++)
            {
                var from = i * PartSize;
                var to = (i == PartCount - 1 ? from + LastPartSize : from + PartSize) - 1;
                yield return new Part(from, to, i + 1);
            }
        }


        private static long CalculatePartSize(long size)
        {
            var partSize = (double)Math.Ceiling((decimal)size / MinioLimitation.MaxPartCount);
            return (long)Math.Ceiling((decimal)partSize / MinioLimitation.MinPartSize) * MinioLimitation.MinPartSize;
        }

    }
}