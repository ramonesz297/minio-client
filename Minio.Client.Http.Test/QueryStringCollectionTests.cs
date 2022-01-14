using Minio.Client.Http.Internal;
using Xunit;

namespace Minio.Client.Http.Test
{
    public class QueryStringCollectionTests
    {
        [Fact]
        public void Should_build_from_query_and_trim_start()
        {
            QueryStringCollection a = new QueryStringCollection("?a=1&b=2", false);

            Assert.Equal("a=1&b=2", a.ToString());
        }

        [Fact]
        public void Should_build_from_query_with_added_parameter()
        {
            QueryStringCollection a = new QueryStringCollection("?a=1&b=2", false);
            a.Add("c", "3");
            Assert.Equal("a=1&b=2&c=3", a.ToString());
        }

        [Fact]
        public void Should_override_parameter()
        {
            QueryStringCollection a = new QueryStringCollection("?a=1&b=2", false);
            a.Add("a", "123");
            Assert.Equal("a=123&b=2", a.ToString());
        }

        [Fact]
        public void Should_be_sorted()
        {
            QueryStringCollection a = new QueryStringCollection(false);
            a.Add("c", "3");
            a.Add("b", "2");
            a.Add("a", "1");
            Assert.Equal("a=1&b=2&c=3", a.ToString());
        }
    }


}