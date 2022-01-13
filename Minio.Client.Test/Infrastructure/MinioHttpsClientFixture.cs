namespace Minio.Client.Test.Infrastructure
{
    public class MinioHttpsClientFixture : BaseMinioHttpClientFixture
    {
        public override bool IsHttps => true;
    }


}