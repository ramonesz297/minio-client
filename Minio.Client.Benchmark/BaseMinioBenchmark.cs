using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Minio.Client.Benchmark
{
    public abstract class BaseMinioBenchmark
    {
        protected MinioClient _restSharpMinioClient = null!;
        protected ServiceProvider _serviceProvider = null!;
        protected MinioHttpClient _client = null!;

        [GlobalSetup]
        public virtual void Setup()
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
        }

        [GlobalCleanup]
        public virtual void CleanUp()
        {
            _serviceProvider.Dispose();
        }
    }
}