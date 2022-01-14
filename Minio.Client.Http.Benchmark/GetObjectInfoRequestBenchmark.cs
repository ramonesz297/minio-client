using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Minio.Client.Http.Model;
using Minio.DataModel;
using System.Threading.Tasks;

namespace Minio.Client.Http.Benchmark
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net472)]
    [SimpleJob(RuntimeMoniker.Net60)]
    public class GetObjectInfoRequestBenchmark : BaseMinioBenchmark
    {
        [Benchmark(Baseline = true)]
        public async Task<ObjectStat> Restsharp()
        {
            return await _restSharpMinioClient.StatObjectAsync(BucketName, FileName);
        }

        [Benchmark]
        public async Task<ObjectInformation> HttpClient()
        {
            return await _client.GetObjectInfoAsync(BucketName, FileName);
        }
    }
}