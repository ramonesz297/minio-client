using Minio.Client.Test.Infrastructure;
using Xunit;

namespace Minio.Client.Test
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