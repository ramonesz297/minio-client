using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Minio.Client.Http.Extensions;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Minio.Client.Http.Benchmark
{

    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net70)]
    [SimpleJob(RuntimeMoniker.Net60)]
    public class PresignedRequestBenchmark : BaseMinioBenchmark
    {
        private Uri _baseUrl = null!;

        private MinioOptions _options = null!;

        public override async Task Setup()
        {
            _baseUrl = new Uri("https://play.min.io");
            _options = new MinioOptions()
            {
                AccessKey = "asdasdas",
                SecretKey = "asdasdasd"
            };
            await base.Setup();
        }


        [Benchmark(Baseline = true, OperationsPerInvoke = 100)]
        public async Task<string> Restsharp()
        {
            return await _restSharpMinioClient.PresignedGetObjectAsync(new PresignedGetObjectArgs().WithBucket(BucketName).WithObject(FileName).WithExpiry(777));
        }

        [Benchmark(OperationsPerInvoke = 100)]
        public Uri? HttpClient_url()
        {
            return new Uri(_baseUrl, $"{BucketName}/{FileName}").PresignUrl(HttpMethod.Get, 777, _options);
        }
    }
}