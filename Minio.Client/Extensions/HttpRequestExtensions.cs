using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Minio.Client
{
    public static class HttpRequestExtensions
    {
        public static async Task<T> ReadAsAsync<T>(this HttpContent content)
        {
            XmlSerializer s = new XmlSerializer(typeof(T));
            return (T)s.Deserialize(await content.ReadAsStreamAsync().ConfigureAwait(false));
        }
    }
}