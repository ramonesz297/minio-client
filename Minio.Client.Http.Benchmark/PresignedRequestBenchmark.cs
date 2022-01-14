using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Minio.Client.Http;
using Minio.Client.Http.Extensions;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Minio.Client.Http.Benchmark
{

    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net472)]
    [SimpleJob(RuntimeMoniker.Net60)]
    public class PresignedRequestBenchmark : BaseMinioBenchmark
    {
        private Uri baseUrl = null!;

        private MinioOptions options = null!;

        public override async Task Setup()
        {
            baseUrl = new Uri("https://play.min.io");
            options = new MinioOptions()
            {
                AccessKey = "asdasdas",
                SecretKey = "asdasdasd"
            };
            await base.Setup();
        }


        [Benchmark(Baseline = true, OperationsPerInvoke = 100)]
        public async Task<string> Restsharp()
        {
            return await _restSharpMinioClient.PresignedGetObjectAsync(BucketName, FileName, 777);
        }

        [Benchmark(OperationsPerInvoke = 100)]
        public Uri? HttpClient_url()
        {
            return new Uri(baseUrl, $"{BucketName}/{FileName}").PresignUrl(HttpMethod.Get, 777, options);
        }
    }
}