using Minio.Client.Http.Internal;
using System.Linq;
using Xunit;

namespace Minio.Client.Http.Test
{

    public class PartSizerTests
    {
        [Theory]
        [InlineData(6 * 1024 * 1024, 1024 * 1024)]//6Mb
        [InlineData(7 * 1024 * 1024, 2 * 1024 * 1024)]//7Mb
        [InlineData(10 * 1024 * 1024, 0)]//10Mb
        public void Should_return_2_parts(long size, long lastPartSize)
        {
            var sizer = new PartSizer(size);
            Assert.Equal(2, sizer.PartCount);
            Assert.Equal(MinioLimitation.MinPartSize, sizer.PartSize);
            Assert.Equal(lastPartSize, sizer.LastPartSize);
        }


        [Theory]
        [InlineData(5 * 1024 * 1024 + 1, 1)]//5Mb + 1b
        public void Should_return_calculate_last_size(long size, long lastPartSize)
        {
            var sizer = new PartSizer(size, MinioLimitation.MinPartSize);
            Assert.Equal(MinioLimitation.MinPartSize, sizer.PartSize);
            Assert.Equal(lastPartSize, sizer.LastPartSize);
        }

        [Fact]
        public void Should_return_correct_parts()
        {
            var size = 6L * 1024 * 1024;

            var sizer = new PartSizer(size);

            var parts = sizer.GetParts().ToList();
            Assert.Equal(1, parts[0].PartId);
            Assert.Equal(0, parts[0].From);
            Assert.Equal(5L * 1024 * 1024 - 1, parts[0].To);

            Assert.Equal(2, parts[1].PartId);
            Assert.Equal(5L * 1024 * 1024, parts[1].From);
            Assert.Equal(6L * 1024 * 1024 - 1, parts[1].To);
        }
    }


}