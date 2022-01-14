using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Minio.Client.Http.DependencyInjection
{
    public static class MinioHttpDependencyInjectionExtensions
    {
        public static IHttpClientBuilder AddMinioHttpClient(this IServiceCollection services, Uri minioHost, Action<MinioOptions> configureOption)
        {
            services.AddOptions<MinioOptions>().Configure(configureOption);

            return services.AddMinioHttpClient(minioHost);
        }

        public static IHttpClientBuilder AddMinioHttpClient(this IServiceCollection services, string name, Uri minioHost, Action<MinioOptions> configureOption)
        {
            services.AddOptions<MinioOptions>().Configure(configureOption);

            return services.AddMinioHttpClient(name, minioHost);
        }

        public static IHttpClientBuilder AddMinioHttpClient(this IServiceCollection services, Uri minioHost)
        {
            services.TryAddTransient<AuthenticationDelegatingHandler>();
            return services.AddHttpClient<IMinioHttpClient, MinioHttpClient>(o =>
             {
                 o.BaseAddress = minioHost;

             }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
        }

        public static IHttpClientBuilder AddMinioHttpClient(this IServiceCollection services, string name, Uri minioHost)
        {
            services.TryAddTransient<AuthenticationDelegatingHandler>();

            return services.AddHttpClient<IMinioHttpClient, MinioHttpClient>(name, o =>
             {
                 o.BaseAddress = minioHost;
             }).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
        }
    }
}