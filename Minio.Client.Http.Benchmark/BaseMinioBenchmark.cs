using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Minio.Client.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Minio.Client.Http.Benchmark
{
    public abstract class BaseMinioBenchmark
    {
        protected MinioClient _restSharpMinioClient = null!;
        protected ServiceProvider _serviceProvider = null!;
        protected MinioHttpClient _client = null!;

        protected string BucketName = "minio-client";

        protected string FileName = "mini-client.txt";

        [GlobalSetup]
        public virtual async Task Setup()
        {
            _restSharpMinioClient = new MinioClient("play.min.io", "Q3AM3UQ867SPQQA43P2F", "zuf+tfteSlswRu7BJ86wekitnifILbZam1KYY3TG")
                .WithSSL();

            var sc = new ServiceCollection();

            sc.AddOptions().Configure<MinioOptions>((o) =>
            {
                o.AccessKey = "Q3AM3UQ867SPQQA43P2F";
                o.SecretKey = "zuf+tfteSlswRu7BJ86wekitnifILbZam1KYY3TG";
            });
            sc.AddTransient<AuthenticationDelegatingHandler>();

            sc.AddHttpClient<MinioHttpClient>((o) =>
            {
                o.BaseAddress = new Uri("https://play.min.io");
            }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();

            _serviceProvider = sc.BuildServiceProvider();

            _client = _serviceProvider.GetRequiredService<MinioHttpClient>();

            if (!await _client.BucketExistAsync(BucketName))
            {
                await _client.CreateBucketAsync(BucketName);
            }

            using var ms = new MemoryStream();
            var payload = Guid.NewGuid().ToByteArray();
            ms.Write(payload, 0, payload.Length);

            ms.Seek(0, SeekOrigin.Begin);

            await _client.PutObjectAsync(BucketName, FileName, new MinioFileRequest(ms));

        }

        [GlobalCleanup]
        public virtual void CleanUp()
        {
            _serviceProvider.Dispose();
        }
    }
}