using System.Net.Http;
using System.Threading.Tasks;

namespace CloudAPI
{
    public interface IHttpClient
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
    }
}
