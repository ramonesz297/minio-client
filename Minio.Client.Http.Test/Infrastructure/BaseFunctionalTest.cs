using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Minio.Client.Http.Test.Infrastructure
{
    public abstract class BaseFunctionalTest<T> where T : BaseMinioHttpClientFixture
    {
        protected abstract T Fixitire { get; }

        [Fact]
        public async Task GetBuckets()
        {
            var list = await Fixitire.Client.GetBucketsAsync();

            Assert.NotNull(list);
        }

        [Fact]
        public async Task Should_BucketExists()
        {
            var result = await Fixitire.Client.BucketExistAsync(Fixitire.BucketName);

            Assert.True(result);
        }


        [Fact]
        public async Task Should_Bucket_do_not_Exist()
        {
            var result = await Fixitire.Client.BucketExistAsync(Guid.NewGuid().ToString());

            Assert.False(result);
        }

        [Theory]
        [InlineData("asdv")]
        [InlineData("12313")]
        [InlineData("a.a")]
        public async Task Should_create_and_delete_Bucket(string sufix)
        {
            var backetName = Guid.NewGuid().ToString() + sufix;

            await Fixitire.Client.CreateBucketAsync(backetName);

            Assert.True(await Fixitire.Client.BucketExistAsync(backetName));

            await Fixitire.Client.RemoveBucketAsync(backetName);

            Assert.False(await Fixitire.Client.BucketExistAsync(backetName));

        }

        [Fact]
        public async Task Should_read_object()
        {
            using var result = await Fixitire.Client.GetObjectAsync(Fixitire.BucketName, Fixitire.FileName);

            var b = new Guid(await result.Content.ReadAsByteArrayAsync());

            Assert.Equal(Fixitire.FileContent, b);
        }


        [Fact]
        public async Task Should_return_presigned_get_url()
        {
            using var result = Fixitire.Client.PresignedGetObjectRequest(Fixitire.BucketName, Fixitire.FileName, 777);

            using var c = new HttpClient();

            var response = await c.SendAsync(result);

            var b = new Guid(await response.Content.ReadAsByteArrayAsync());

            Assert.Equal(Fixitire.FileContent, b);
        }


        [Fact]
        public async Task Should_return_object_info()
        {
            var result = await Fixitire.Client.GetObjectInfoAsync(Fixitire.BucketName, Fixitire.FileName);
            Assert.Equal(result.ObjectName, Fixitire.FileName);
            Assert.Equal(result.Size, Fixitire.FileContent.ToByteArray().Length);
        }


        [Theory]
        [InlineData("test-object-name.txt")]
        [InlineData("test/test-object-name.txt")]
        [InlineData("test /test object name.txt")]
        [InlineData("test/test object name.txt")]
        public async Task Should_return_presigned_valid_put_url(string filename)
        {
            long length = 0;
            using var result = Fixitire.Client.PresignedPutObjectRequest(Fixitire.BucketName, filename, 777);

            using var c = new HttpClient();
            using var ms = new MemoryStream();
            using var content = new StreamContent(ms);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");
            using (var sw = new StreamWriter(ms, System.Text.Encoding.UTF8, 1024, true))
            {
                sw.WriteLine("line of text");
                await sw.FlushAsync();
            }
            ms.Seek(0, SeekOrigin.Begin);
            length = ms.Length;
            var data = await c.PutAsync(result.RequestUri, content);

            var info = await Fixitire.Client.GetObjectInfoAsync(Fixitire.BucketName, filename);

            Assert.Equal(info.Size, length);

        }

        [Theory]
        [InlineData("some.txt")]
        [InlineData("some .txt")]
        [InlineData("some()$%.txt")]
        [InlineData("s ?:%/ome()$%.txt")]
        public async Task Should_upload_object(string fileName)
        {
            using var ms = new MemoryStream();

            using (var sw = new StreamWriter(ms, System.Text.Encoding.UTF8, 1024, true))
            {
                sw.WriteLine("line of text");
                await sw.FlushAsync();
            }

            ms.Seek(0, SeekOrigin.Begin);

            var result = await Fixitire.Client.CorePutObjectAsync(Fixitire.BucketName, fileName, new MinioFileRequest(ms));

            var info = await Fixitire.Client.GetObjectInfoAsync(Fixitire.BucketName, fileName);
            Assert.NotEqual(0, info.Size);
            Assert.NotNull(info.ETag);
            Assert.Equal(info.ETag, result.ETag);
        }

        [Fact]
        public async Task Should_upload_object_multipart()
        {

            using var ms = new MemoryStream();
            var guid = Guid.NewGuid().ToByteArray();

            while (ms.Length < 6 * 1024 * 1024)
            {
                ms.Write(guid, 0, guid.Length);
            }
            
            ms.Write(new byte[] { 1 }, 0, 1);

            ms.Seek(0, SeekOrigin.Begin);

            string name = Guid.NewGuid().ToString() + ".txt";


            var result = await Fixitire.Client.PutObjectAsync(Fixitire.BucketName, name, new MinioFileRequest(ms));

            var info = await Fixitire.Client.GetObjectInfoAsync(Fixitire.BucketName, name);

            Assert.NotEqual(0, info.Size);
            Assert.NotNull(info.ETag);
        }

        [Fact]
        public async Task Should_return_upload_id()
        {
            string name = Guid.NewGuid().ToString() + ".txt";

            var id = await Fixitire.Client.InitializeMulipartUploadAsync(Fixitire.BucketName, name);

            Assert.NotNull(id);
            Assert.NotEmpty(id);
        }

        [Fact]
        public async Task Should_copy()
        {
            var destinationFile = Guid.NewGuid().ToString() + ".txt";

            await Fixitire.Client.CopyAsync(new CopyRequest(Fixitire.BucketName,
                                                      Fixitire.FileName,
                                                      Fixitire.BucketName,
                                                      destinationFile));

            var result = await Fixitire.Client.GetObjectInfoAsync(Fixitire.BucketName, destinationFile);

            Assert.Equal(Fixitire.FileContent.ToByteArray().Length, result.Size);
        }

        [Fact]
        public async Task Should_copy_with_unescaped_characters()
        {
            var destinationFile = Guid.NewGuid().ToString() + "тест тест.txt";

            await Fixitire.Client.CopyAsync(new CopyRequest(Fixitire.BucketName,
                                                      Fixitire.FileNameCyrillic,
                                                      Fixitire.BucketName,
                                                      destinationFile));

            var result = await Fixitire.Client.GetObjectInfoAsync(Fixitire.BucketName, destinationFile);

            Assert.Equal(Fixitire.FileContent.ToByteArray().Length, result.Size);
        }


        [Fact]
        public async Task Should_copy_multipart()
        {
            var destinationFile = Guid.NewGuid().ToString() + ".txt";

            await Fixitire.Client.CopyAsync(new CopyRequest(Fixitire.BucketName,
                                                      Fixitire.BigFileName,
                                                      Fixitire.BucketName,
                                                      destinationFile, maxSingleSizeUpload: 5 * 1024 * 1024));

            var result = await Fixitire.Client.GetObjectInfoAsync(Fixitire.BucketName, destinationFile);

            Assert.NotNull(result.ETag);
            Assert.Equal(result.Size, 6 * 1024 * 1024L);
        }

        [Fact]
        public async Task Should_return_list_of_objects_with_prefix()
        {

            var result = await Fixitire.Client.GetObjectsAsync(new GetObjectsRequests(Fixitire.BucketName, recursive: true, prefix: "data", maxKeys: 1)).ToListAsync();

            Assert.Equal(4, result.Count);

            foreach (var item in result)
            {
                var info = await Fixitire.Client.GetObjectInfoAsync(Fixitire.BucketName, item.ObjectName);
                Assert.Equal(info.ETag, item.ETag);
                Assert.Equal(info.Size, item.Size);
                Assert.NotNull(item.LastModified);
                Assert.Equal(info.LastModified.GetValueOrDefault().UtcDateTime, item.LastModified.GetValueOrDefault().UtcDateTime, TimeSpan.FromSeconds(1.0));
            }

        }


        [Fact]
        public async Task Should_return_list_of_objects_without_prefix()
        {

            var result = await Fixitire.Client.GetObjectsAsync(new GetObjectsRequests(Fixitire.BucketName, recursive: true, maxKeys: 1)).ToListAsync();

            foreach (var item in result)
            {
                var info = await Fixitire.Client.GetObjectInfoAsync(Fixitire.BucketName, item.ObjectName);
                Assert.Equal(info.ETag, item.ETag);
                Assert.Equal(info.Size, item.Size);
                Assert.NotNull(item.LastModified);
                Assert.Equal(info.LastModified.GetValueOrDefault().UtcDateTime, item.LastModified.GetValueOrDefault().UtcDateTime, TimeSpan.FromSeconds(1.0));
            }
        }

    }
}