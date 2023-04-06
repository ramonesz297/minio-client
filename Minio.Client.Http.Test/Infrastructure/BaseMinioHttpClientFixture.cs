using Microsoft.Extensions.DependencyInjection;
using Minio.Client.Http.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
namespace Minio.Client.Http.Test.Infrastructure
{
    public abstract class BaseMinioHttpClientFixture : IAsyncLifetime
    {
        protected ServiceProvider sp;

        public string BucketName { get; } = Guid.NewGuid().ToString();

        public string FileName { get; } = Guid.NewGuid().ToString() + ".txt";


        public string FileNameCyrillic { get; } = Guid.NewGuid().ToString() + "тест назва.txt";

        public string BigFileName { get; } = Guid.NewGuid().ToString() + ".txt";

        public IMinioHttpClient Client => sp.GetRequiredService<IMinioHttpClient>();

        public Guid FileContent { get; } = Guid.NewGuid();

        public abstract bool IsHttps { get; }

        public BaseMinioHttpClientFixture()
        {
            IServiceCollection sc = new ServiceCollection();

            var url = IsHttps ? new Uri("https://play.min.io") : new Uri("http://play.min.io");

            sc.AddMinioHttpClient(url, o =>
            {
                o.AccessKey = "Q3AM3UQ867SPQQA43P2F";
                o.SecretKey = "zuf+tfteSlswRu7BJ86wekitnifILbZam1KYY3TG";
            });

            sp = sc.BuildServiceProvider();
        }

        public async Task InitializeAsync()
        {
            await Client.CreateBucketAsync(BucketName);

            using var ms = new MemoryStream(FileContent.ToByteArray());
            ms.Seek(0, SeekOrigin.Begin);

            var result = await Client.PutObjectAsync(BucketName, FileName, new MinioFileRequest(ms), true);

            ms.Seek(0, SeekOrigin.Begin);

            await Client.PutObjectAsync(BucketName, FileNameCyrillic, new MinioFileRequest(ms), true);

            using var ms2 = new MemoryStream();
            var guid = Guid.NewGuid().ToByteArray();

            while (ms2.Length < 6 * 1024 * 1024)
            {
                ms2.Write(guid, 0, guid.Length);
            }
            ms2.Seek(0, SeekOrigin.Begin);
            result = await Client.PutObjectAsync(BucketName, BigFileName, new MinioFileRequest(ms2));


            ms.Seek(0, SeekOrigin.Begin);

            await Client.PutObjectAsync(BucketName, $"data/1 абс/{FileName}", new MinioFileRequest(ms), true);

            ms.Seek(0, SeekOrigin.Begin);

            await Client.PutObjectAsync(BucketName, $"data/2 абс/{FileName}", new MinioFileRequest(ms), true);

            ms.Seek(0, SeekOrigin.Begin);

            await Client.PutObjectAsync(BucketName, $"data/3 абс/{FileName}", new MinioFileRequest(ms), true);

            ms.Seek(0, SeekOrigin.Begin);

            await Client.PutObjectAsync(BucketName, $"data/4 абс/{FileName}", new MinioFileRequest(ms), true);
        }

        public async Task DisposeAsync()
        {
            await sp.DisposeAsync();
        }
    }


}