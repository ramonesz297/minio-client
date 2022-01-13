using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Minio.Client.Model;
using Minio.DataModel;
using System.Threading.Tasks;

namespace Minio.Client.Benchmark
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net472)]
    [SimpleJob(RuntimeMoniker.Net60)]
    public class GetObjectInfoRequestBenchmark : BaseMinioBenchmark
    {
        [Benchmark(Baseline = true)]
        public async Task<ObjectStat> Restsharp()
        {
            return await _restSharpMinioClient.StatObjectAsync("mynewbucket", "blablabla.txt");
        }

        [Benchmark]
        public async Task<ObjectInformation> HttpClient()
        {
            return await _client.GetObjectInfoAsync("mynewbucket", "blablabla.txt");
        }
    }
}