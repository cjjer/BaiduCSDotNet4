using Newtonsoft.Json.Linq;

namespace CloudAPI
{
    internal static class JTokenExtensions
    {
        internal static string AsString(this JToken token)
        {
            return token.Type != JTokenType.String
                ? token.ToString()
                : token.Value<string>();
        }
    }
}
