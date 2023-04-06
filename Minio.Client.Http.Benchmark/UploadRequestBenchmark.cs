using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Minio.Client.Http.Benchmark
{
    [MemoryDiagnoser]
    [SimpleJob(RunStrategy.Monitoring, RuntimeMoniker.Net60)]
    [SimpleJob(RunStrategy.Monitoring, RuntimeMoniker.Net70)]
    public class UploadRequestBenchmark : BaseMinioBenchmark
    {
        private MemoryStream _content = null!;
        private MemoryStream _content2 = null!;

        public override async Task Setup()
        {
            await base.Setup();
            _content = new MemoryStream(10 * 1024 * 1024);
            _content2 = new MemoryStream(10 * 1024 * 1024);
            var guid = Guid.NewGuid().ToByteArray();
            while (_content.Length < 10 * 1024 * 1024)
            {
                _content.Write(guid, 0, guid.Length);
                _content2.Write(guid, 0, guid.Length);
            }
            _content.Seek(0, SeekOrigin.Begin);
            _content2.Seek(0, SeekOrigin.Begin);
        }

        public override void CleanUp()
        {
            base.CleanUp();
            _content.Dispose();
            _content2.Dispose();
        }


        [Benchmark(Baseline = true)]
        public async Task Restsharp()
        {
            _content.Seek(0, SeekOrigin.Begin);
            await _restSharpMinioClient.PutObjectAsync(new PutObjectArgs().WithBucket(BucketName).WithFileName($"{Guid.NewGuid()}.txt").WithStreamData(_content).WithObjectSize(_content.Length));
        }

        [Benchmark()]
        public async Task HttpClient()
        {
            _content2.Seek(0, SeekOrigin.Begin);
            await _client.PutObjectAsync(BucketName, $"{Guid.NewGuid()}.txt", new MinioFileRequest(_content2), true);
        }
    }
}