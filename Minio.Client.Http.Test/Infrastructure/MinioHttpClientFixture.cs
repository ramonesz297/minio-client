namespace Minio.Client.Http.Test.Infrastructure
{

    public class MinioHttpClientFixture : BaseMinioHttpClientFixture
    {
        public override bool IsHttps => false;
    }


}