using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Minio.Client.Benchmark
{
    [MemoryDiagnoser]
    [SimpleJob(RunStrategy.Monitoring, RuntimeMoniker.Net472, targetCount: 20)]
    [SimpleJob(RunStrategy.Monitoring, RuntimeMoniker.Net60, targetCount: 20)]
    public class UploadRequestBenchmark : BaseMinioBenchmark
    {
        private MemoryStream content = null!;
        private MemoryStream content2 = null!;

        public override void Setup()
        {
            base.Setup();
            content = new MemoryStream(10 * 1024 * 1024);
            content2 = new MemoryStream(10 * 1024 * 1024);
            var guid = Guid.NewGuid().ToByteArray();
            while (content.Length < 10 * 1024 * 1024)
            {
                content.Write(guid, 0, guid.Length);
                content2.Write(guid, 0, guid.Length);
            }
            content.Seek(0, SeekOrigin.Begin);
            content2.Seek(0, SeekOrigin.Begin);
        }

        public override void CleanUp()
        {
            base.CleanUp();
            content.Dispose();
            content2.Dispose();
        }


        [Benchmark(Baseline = true)]
        public async Task Restsharp()
        {
            content.Seek(0, SeekOrigin.Begin);
            await _restSharpMinioClient.PutObjectAsync("mynewbucket", "blablabla.txt", content, content.Length);
        }

        [Benchmark()]
        public async Task HttpClient()
        {
            content2.Seek(0, SeekOrigin.Begin);
            await _client.PutObjectAsync("mynewbucket", "blablabla.txt", new MinioFileRequest(content2), true);
        }
    }
}