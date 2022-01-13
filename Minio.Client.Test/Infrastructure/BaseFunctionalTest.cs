using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Minio.Client.Test.Infrastructure
{
    public abstract class BaseFunctionalTest<T> where T : BaseMinioHttpClientFixture
    {
        protected abstract T Fxitire { get; }

        [Fact]
        public async Task GetBuckets()
        {
            var list = await Fxitire.Client.GetBucketsAsync();

            Assert.NotNull(list);
        }

        [Fact]
        public async Task Should_BucketExists()
        {
            var result = await Fxitire.Client.BucketExistAsync(Fxitire.BucketName);

            Assert.True(result);
        }


        [Fact]
        public async Task Should_Bucket_do_not_Exist()
        {
            var result = await Fxitire.Client.BucketExistAsync(Guid.NewGuid().ToString());

            Assert.False(result);
        }

        [Fact]
        public async Task Should_create_and_delete_Bucket()
        {
            var backetName = Guid.NewGuid().ToString();

            await Fxitire.Client.CreateBucketAsync(backetName);

            Assert.True(await Fxitire.Client.BucketExistAsync(backetName));

            await Fxitire.Client.RemoveBucketAsync(backetName);

            Assert.False(await Fxitire.Client.BucketExistAsync(backetName));

        }

        [Fact]
        public async Task Should_read_object()
        {
            using var result = await Fxitire.Client.GetObjectAsync(Fxitire.BucketName, Fxitire.FileName);

            var b = new Guid(await result.Content.ReadAsByteArrayAsync());

            Assert.Equal(Fxitire.FileContent, b);
        }


        [Fact]
        public async Task Should_return_presigned_get_url()
        {
            using var result = Fxitire.Client.PresignedGetObjectRequest(Fxitire.BucketName, Fxitire.FileName, 777);

            using var c = new HttpClient();

            var response = await c.SendAsync(result);

            var b = new Guid(await response.Content.ReadAsByteArrayAsync());

            Assert.Equal(Fxitire.FileContent, b);
        }


        [Fact]
        public async Task Should_return_object_info()
        {
            var result = await Fxitire.Client.GetObjectInfoAsync(Fxitire.BucketName, Fxitire.FileName);
            Assert.Equal(result.ObjectName, Fxitire.FileName);
            Assert.Equal(result.Size, Fxitire.FileContent.ToByteArray().Length);
        }


        [Fact]
        public async Task Should_return_presigned_valid_put_url()
        {
            long length = 0;
            using var result = Fxitire.Client.PresignedPutObjectRequest(Fxitire.BucketName, "test-object-name.txt", 777);

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

            var info = await Fxitire.Client.GetObjectInfoAsync(Fxitire.BucketName, "test-object-name.txt");

            Assert.Equal(info.Size, length);

        }

        [Fact]
        public async Task Should_upload_object()
        {

            using var ms = new MemoryStream();

            using (var sw = new StreamWriter(ms, System.Text.Encoding.UTF8, 1024, true))
            {
                sw.WriteLine("line of text");
                await sw.FlushAsync();
            }

            ms.Seek(0, SeekOrigin.Begin);

            string name = Guid.NewGuid().ToString() + ".txt";

            var result = await Fxitire.Client.CorePutObjectAsync(Fxitire.BucketName, name, new MinioFileRequest(ms));

            var info = await Fxitire.Client.GetObjectInfoAsync(Fxitire.BucketName, name);
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

            ms.Seek(0, SeekOrigin.Begin);

            string name = Guid.NewGuid().ToString() + ".txt";

            var result = await Fxitire.Client.PutObjectAsync(Fxitire.BucketName, name, new MinioFileRequest(ms));

            var info = await Fxitire.Client.GetObjectInfoAsync(Fxitire.BucketName, name);

            Assert.NotEqual(0, info.Size);
            Assert.NotNull(info.ETag);
        }

        [Fact]
        public async Task Should_return_upload_id()
        {
            string name = Guid.NewGuid().ToString() + ".txt";

            var id = await Fxitire.Client.InitializeMulipartUploadAsync(Fxitire.BucketName, name);

            Assert.NotNull(id);
            Assert.NotEmpty(id);
        }

        [Fact]
        public async Task Should_copy()
        {
            var destinationFile = Guid.NewGuid().ToString() + ".txt";

            await Fxitire.Client.CopyAsync(new CopyRequest(Fxitire.BucketName,
                                                      Fxitire.FileName,
                                                      Fxitire.BucketName,
                                                      destinationFile));

            var result = await Fxitire.Client.GetObjectInfoAsync(Fxitire.BucketName, destinationFile);

            Assert.Equal(Fxitire.FileContent.ToByteArray().Length, result.Size);
        }


        [Fact]
        public async Task Should_copy_multipart()
        {
            var destinationFile = Guid.NewGuid().ToString() + ".txt";

            await Fxitire.Client.CopyAsync(new CopyRequest(Fxitire.BucketName,
                                                      Fxitire.BigFileName,
                                                      Fxitire.BucketName,
                                                      destinationFile, maxSingleSizeUpload: 5 * 1024 * 1024));

            var result = await Fxitire.Client.GetObjectInfoAsync(Fxitire.BucketName, destinationFile);

            Assert.NotNull(result.ETag);
            Assert.Equal(result.Size, 6 * 1024 * 1024L);
        }
    }


}