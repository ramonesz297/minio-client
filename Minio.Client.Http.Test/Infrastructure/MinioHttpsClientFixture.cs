namespace Minio.Client.Http.Test.Infrastructure
{
    public class MinioHttpsClientFixture : BaseMinioHttpClientFixture
    {
        public override bool IsHttps => true;
    }


}