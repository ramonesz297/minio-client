using Minio.Client.Test.Infrastructure;
using Xunit;

namespace Minio.Client.Test
{
    public class FunctionalHttpTests : BaseFunctionalTest<MinioHttpClientFixture>, IClassFixture<MinioHttpClientFixture>
    {

        public FunctionalHttpTests(MinioHttpClientFixture fxitire)
        {
            Fxitire = fxitire;
        }

        protected override MinioHttpClientFixture Fxitire { get; }
    }


}