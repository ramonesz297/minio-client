using BenchmarkDotNet.Running;

namespace Minio.Client.Http.Benchmark
{

    public static class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run(typeof(Program).Assembly, args: args);
        }
    }
}