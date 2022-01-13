namespace Minio.Client.Test.Infrastructure
{

    public class MinioHttpClientFixture : BaseMinioHttpClientFixture
    {
        public override bool IsHttps => false;
    }


}