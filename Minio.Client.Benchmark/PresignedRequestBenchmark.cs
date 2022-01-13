using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Minio.Client.Benchmark
{

    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net472)]
    [SimpleJob(RuntimeMoniker.Net60)]
    public class PresignedRequestBenchmark : BaseMinioBenchmark
    {
        private static readonly Uri baseUrl = new Uri("https://play.min.io");
        private static readonly MinioOptions o = new MinioOptions()
        {
            AccessKey = "asdasdas",
            SecretKey = "asdasdasd"
        };


        [Benchmark(Baseline = true, OperationsPerInvoke = 1000)]
        public async Task<string> Restsharp()
        {
            return await _restSharpMinioClient.PresignedGetObjectAsync("mynewbucket", "blablabla.txt", 777);
        }

        [Benchmark(OperationsPerInvoke = 1000)]
        public Uri? HttpClient_url()
        {
            return new Uri(baseUrl, "mynewbucket/blablabla.txt").PresignUrl(HttpMethod.Get, 777, o);
        }

        [Benchmark(OperationsPerInvoke = 1000)]
        public HttpRequestMessage HttpClient_request()
        {

            return _client.PresignedGetObjectRequest("mynewbucket", "blablabla.txt", 777);
        }
    }
}