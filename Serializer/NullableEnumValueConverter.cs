using System;
using Newtonsoft.Json;

namespace CloudAPI.Serializer
{
    public class NullableEnumValueConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (string.IsNullOrEmpty(reader.Value.ToString()))
                return null;

            var enumType = Nullable.GetUnderlyingType(objectType);
            return Enum.Parse(enumType, reader.Value.ToString());
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsGenericType &&
                   objectType.GetGenericTypeDefinition() == typeof (Nullable<>) &&
                   Nullable.GetUnderlyingType(objectType).IsEnum;
        }
    }
}