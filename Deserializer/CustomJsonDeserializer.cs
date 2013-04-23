using System.Collections;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace CloudAPI.Deserializer
{
    public class CustomJsonDeserializer
    {
        readonly CultureInfo culture = CultureInfo.InvariantCulture;

        public T Deserialize<T>(string content) where T : new()
        {
            content = CommonDeserializerMethods.ReplaceAllDateInstacesWithNeoDates(content);
            //此处检查 N4 返回单个数据的时候没有加 [] 符号引起数据非列表失败
             var target = new T();
             if (target is IList || target is IDictionary)
             {
                 if (!content.StartsWith("[")) content = "[" + content + "]";
             }
            var reader = new JsonTextReader(new StringReader(content))
            {
                DateParseHandling = DateParseHandling.DateTimeOffset
            };


            if (target is IList)
            {
                var objType = target.GetType();
                var json = JToken.ReadFrom(reader);

                target = (T)CommonDeserializerMethods.BuildList(objType, json.Root.Children(), culture, new TypeMapping[0], 0);

            }
            else if (target is IDictionary)
            {
                var root = JToken.ReadFrom(reader).Root;
                target = (T)CommonDeserializerMethods.BuildDictionary(target.GetType(), root.Children(), culture, new TypeMapping[0], 0);
            }
            else
            {
                var root = JToken.ReadFrom(reader).Root;
                CommonDeserializerMethods.Map(target, root, culture, new TypeMapping[0], 0);
            }

            return target;
        }
    }
}
