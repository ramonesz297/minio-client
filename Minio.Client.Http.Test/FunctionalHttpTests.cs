using Minio.Client.Http.Test.Infrastructure;
using Xunit;

namespace Minio.Client.Http.Test
{
    public class FunctionalHttpTests : BaseFunctionalTest<MinioHttpClientFixture>, IClassFixture<MinioHttpClientFixture>
    {

        public FunctionalHttpTests(MinioHttpClientFixture fxitire)
        {
            Fixitire = fxitire;
        }

        protected override MinioHttpClientFixture Fixitire { get; }
    }


}