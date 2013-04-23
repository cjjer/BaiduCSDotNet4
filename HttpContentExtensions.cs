using System.Net.Http;
using CloudAPI.Deserializer;

namespace CloudAPI
{
    internal static class HttpContentExtensions
    {
        public static string ReadAsString(this HttpContent content)
        {
            var readTask = content.ReadAsStringAsync();
            readTask.Wait();
            return readTask.Result;
        }

        public static T ReadAsJson<T>(this HttpContent content) where T : new()
        {
            var stringContent = content.ReadAsString();
            return new CustomJsonDeserializer().Deserialize<T>(stringContent);
        }
    }
}
