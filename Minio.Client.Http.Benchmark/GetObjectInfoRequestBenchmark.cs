using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Minio.Client.Http.Model;
using Minio.DataModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Minio.Client.Http.Benchmark
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net70)]
    [SimpleJob(RuntimeMoniker.Net60)]
    public class GetObjectInfoRequestBenchmark : BaseMinioBenchmark
    {
        [Benchmark(Baseline = true)]
        public async Task<ObjectStat> Restsharp()
        {
            return await _restSharpMinioClient.StatObjectAsync(new StatObjectArgs().WithBucket(BucketName).WithObject(FileName));
        }

        [Benchmark]
        public async Task<ObjectInformation> HttpClient()
        {
            return await _client.GetObjectInfoAsync(BucketName, FileName);
        }
    }
}