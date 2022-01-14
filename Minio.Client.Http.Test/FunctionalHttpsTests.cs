using Minio.Client.Http.Test.Infrastructure;
using Xunit;

namespace Minio.Client.Http.Test
{

    public class FunctionalHttpsTests : BaseFunctionalTest<MinioHttpsClientFixture>, IClassFixture<MinioHttpsClientFixture>
    {
        protected override MinioHttpsClientFixture Fxitire { get; }

        public FunctionalHttpsTests(MinioHttpsClientFixture fxitire)
        {
            Fxitire = fxitire;
        }
    }


}