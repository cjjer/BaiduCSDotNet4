﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CloudAPI.Deserializer
{
    class CommonDeserializerMethods
    {
        static readonly Regex DateRegex = new Regex(@"/Date\([-]?\d+([+-]\d+)?\)/");
        static readonly Regex DateTypeNameRegex = new Regex(@"(?<=(?<quote>['""])/)Date(?=\(.*?\)/\k<quote>)");

        public static string ReplaceAllDateInstacesWithNeoDates(string content)
        {
            // Replace all /Date(1234+0200)/ instances with /NeoDate(1234+0200)/
            return DateTypeNameRegex.Replace(content, "Date");
        }

        public static DateTimeOffset? ParseDateTimeOffset(JToken value)
        {
            var jValue = value as JValue;
            if (jValue != null)
            {
                if (jValue.Value == null)
                    return null;

                if (jValue.Value is DateTimeOffset)
                    return jValue.Value<DateTimeOffset>();
            }

            var rawValue = value.AsString();

            if (string.IsNullOrWhiteSpace(rawValue))
                return null;

            rawValue = rawValue.Replace("NeoDate", "Date");

            if (!DateRegex.IsMatch(rawValue))
            {
                DateTimeOffset parsed;
                if (!DateTimeOffset.TryParse(rawValue, out parsed))
                    return null;
            }

            var text = string.Format("{{\"a\":\"{0}\"}}", rawValue);
            var reader = new JsonTextReader(new StringReader(text)) {DateParseHandling = DateParseHandling.DateTimeOffset};
            reader.Read(); // JsonToken.StartObject
            reader.Read(); // JsonToken.PropertyName
            return reader.ReadAsDateTimeOffset();
        }

        public static object CoerceValue(
            PropertyInfo propertyInfo,
            JToken value,
            CultureInfo culture,
            IEnumerable<TypeMapping> typeMappings,
            int nestingLevel)
        {
            if (value == null || value.Type == JTokenType.Null)
                return null;

            var propertyType = propertyInfo.PropertyType;

            // check for nullable and extract underlying type
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                propertyType = propertyType.GetGenericArguments()[0];
            }

            var genericTypeDef = propertyType.IsGenericType ? propertyType.GetGenericTypeDefinition() : null;

            typeMappings = typeMappings.ToArray();
            if (propertyType.IsPrimitive)
            {
                // no primitives can contain quotes so we can safely remove them
                // allows converting a json value like {"index": "1"} to an int
                object tmpVal = value.AsString().Replace("\"", string.Empty);
                tmpVal = Convert.ChangeType(tmpVal, propertyType);
                return tmpVal;
            }

            if (propertyType.IsEnum)
            {
                var raw = value.AsString();
                var converted = Enum.Parse(propertyType, raw, false);
                return converted;
            }

            if (propertyType == typeof(Uri))
            {
                var raw = value.AsString();
                var uri = new Uri(raw, UriKind.RelativeOrAbsolute);
                return uri;
            }

            if (propertyType == typeof(string))
            {
                var raw = value.AsString();
                return raw;
            }

            if (propertyType == typeof(DateTime))
            {
                throw new NotSupportedException("DateTime values are not supported. Use DateTimeOffset instead.");
            }

            if (propertyType == typeof(DateTimeOffset))
            {
                var dateTimeOffset = ParseDateTimeOffset(value);
                if (dateTimeOffset.HasValue)
                    return dateTimeOffset.Value;
                return null;
            }

            if (propertyType == typeof(Decimal))
            {
                var dec = Decimal.Parse(value.AsString(), culture);
                return dec;
            }

            if (propertyType == typeof(TimeSpan))
            {
                var valueString = value.ToString();
                var timeSpan = TimeSpan.Parse(valueString);
                return timeSpan;
            }

            if (propertyType == typeof(Guid))
            {
                var raw = value.AsString();
                var guid = string.IsNullOrEmpty(raw) ? Guid.Empty : new Guid(raw);
                return guid;
            }

            if (genericTypeDef == typeof(List<>))
            {
                var list = BuildList(propertyType, value.Children(), culture, typeMappings, nestingLevel + 1);
                return list;
            }

            if (genericTypeDef == typeof(Dictionary<,>))
            {
                var keyType = propertyType.GetGenericArguments()[0];

                // only supports Dict<string, T>()
                if (keyType != typeof (string))
                {
                    throw new NotSupportedException("Value coersion only supports dictionaries with a key of type System.String");
                }

                var dict = BuildDictionary(propertyType, value.Children(), culture, typeMappings, nestingLevel + 1);
                return dict;
            }

            // nested objects
            var mapping = typeMappings.FirstOrDefault(m => m.ShouldTriggerForPropertyType(nestingLevel, propertyType));
            var item = mapping != null ? MutateObject(value, culture, typeMappings, nestingLevel, mapping, propertyType) : CreateAndMap(propertyType, value, culture, typeMappings, nestingLevel + 1);
            return item;
        }

        public static void SetPropertyValue(
            object targetObject,
            PropertyInfo propertyInfo,
            JToken value,
            CultureInfo culture,
            IEnumerable<TypeMapping> typeMappings,
            int nestingLevel)
        {
            if (value == null || value.Type == JTokenType.Null)
                return;

            var coercedValue = CoerceValue(propertyInfo, value, culture, typeMappings, nestingLevel);
            propertyInfo.SetValue(targetObject, coercedValue, null);
        }

        public static object CreateAndMap(Type type, JToken element, CultureInfo culture, IEnumerable<TypeMapping> typeMappings, int nestingLevel)
        {
            object instance;
            typeMappings = typeMappings.ToArray();
            if (type.IsGenericType)
            {
                var genericTypeDef = type.GetGenericTypeDefinition();
                if (genericTypeDef == typeof (Dictionary<,>))
                {
                    instance = BuildDictionary(type, element.Children(), culture, typeMappings, nestingLevel + 1);
                }
                else if (genericTypeDef == typeof (List<>))
                {
                    instance = BuildList(type, element.Children(), culture, typeMappings, nestingLevel + 1);
                }
                else if (genericTypeDef == typeof (IEnumerable<>))
                {
                    instance = BuildIEnumerable(type, element.Children(), culture, typeMappings, nestingLevel + 1);
                }
                else if (type == typeof (string))
                {
                    instance = (string) element;
                }
                else
                {
                    var mapping = typeMappings.FirstOrDefault(m => m.ShouldTriggerForPropertyType(nestingLevel, type));
                    if (mapping != null)
                        instance = MutateObject(element, culture, typeMappings, nestingLevel, mapping, type);
                    else
                    {
                        instance = Activator.CreateInstance(type);
                        Map(instance, element, culture, typeMappings, nestingLevel);
                    }
                }
            }
            else if (type.IsValueType)
            {
                if (type == typeof(Guid))
                {
                    instance = Guid.Parse(element.ToString());
                }
                else if(type.BaseType == typeof(Enum))
                {
                    instance = Enum.Parse(type, element.ToString(), false);
                }
                else
                {
                    instance = Convert.ChangeType(element.ToString(), type);
                }
            }
            else if(type.BaseType == typeof(Array)) //One Dimensional Only
            {
                var underlyingType = type.GetElementType();
                var arrayType = typeof(ArrayList);
                instance = BuildArray(arrayType, underlyingType, element.Children(), culture, typeMappings, nestingLevel + 1);
            }
            else if (type == typeof(string))
            {
                instance = element.ToString();
            }
            else if (type == typeof(TimeZoneInfo))
            {
                try
                {
                    instance = TimeZoneInfo.FindSystemTimeZoneById((string)element);
                }
                catch
                {
                    Trace.WriteLine("Could not deserialize TimeZoneInfo, defaulting to Utc. Ensure the TimeZoneId is valid. Valid TimeZone Ids are:");
                    foreach (var timeZone in TimeZoneInfo.GetSystemTimeZones())
                    {
                        Trace.WriteLine(timeZone.Id);
                    }
                    instance = TimeZoneInfo.Utc;
                }
            }
            else
            {
                instance = Activator.CreateInstance(type);
                Map(instance, element, culture, typeMappings, nestingLevel);
            }
            return instance;
        }

        static object MutateObject(JToken value, CultureInfo culture, IEnumerable<TypeMapping> typeMappings, int nestingLevel,
                                   TypeMapping mapping, Type propertyType)
        {
            var newType = mapping.DetermineTypeToParseJsonIntoBasedOnPropertyType(propertyType);
            var rawItem = CreateAndMap(newType, value, culture, typeMappings, nestingLevel + 1);
            var item = mapping.MutationCallback(rawItem);
            return item;
        }

        public static void Map(object targetObject, JToken parentJsonToken, CultureInfo culture, IEnumerable<TypeMapping> typeMappings, int nestingLevel)
        {
            typeMappings = typeMappings.ToArray();
            var objType = targetObject.GetType();
            var props = GetPropertiesForType(objType);
            IDictionary<string, JToken> dictionary = parentJsonToken as JObject;
            if (dictionary != null && props.Keys.All(dictionary.ContainsKey) == false && dictionary.ContainsKey("data")) {
               parentJsonToken = parentJsonToken["data"];
            }

            foreach (var propertyName in props.Keys)
            {
                var propertyInfo = props[propertyName];
                var jsonToken = parentJsonToken[propertyName];
                SetPropertyValue(targetObject, propertyInfo, jsonToken, culture, typeMappings, nestingLevel);
            }
        }

        public static IDictionary BuildDictionary(Type type, JEnumerable<JToken> elements, CultureInfo culture, IEnumerable<TypeMapping> typeMappings, int nestingLevel)
        {
            typeMappings = typeMappings.ToArray();
            var dict = (IDictionary)Activator.CreateInstance(type);
            var valueType = type.GetGenericArguments()[1];
            foreach (JProperty child in elements)
            {
                var key = child.Name;
                var item = CreateAndMap(valueType, child.Value, culture, typeMappings, nestingLevel + 1);
                dict.Add(key, item);
            }

            return dict;
        }

        public static IList BuildList(Type type, JEnumerable<JToken> elements, CultureInfo culture, IEnumerable<TypeMapping> typeMappings, int nestingLevel)
        {
            typeMappings = typeMappings.ToArray();
            var list = (IList)Activator.CreateInstance(type);
            var itemType = type
                .GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>))
                .Select(i => i.GetGenericArguments().First())
                .Single();

            foreach (var element in elements)
            {
                if (itemType.IsPrimitive)
                {
                    var value = element as JValue;
                    if (value != null)
                    {
                        list.Add(Convert.ChangeType(value.Value, itemType));
                    }
                }
                else if (itemType == typeof(string))
                {
                    list.Add(element.AsString());
                }
                else
                {
                    var item = CreateAndMap(itemType, element, culture, typeMappings, nestingLevel + 1);
                    list.Add(item);
                }
            }
            return list;
        }

        public static Array BuildArray(Type type, Type itemType,  JEnumerable<JToken> elements, CultureInfo culture, IEnumerable<TypeMapping> typeMappings, int nestingLevel)
        {
            typeMappings = typeMappings.ToArray();
            var list = (ArrayList)Activator.CreateInstance(type);

            foreach (var element in elements)
            {
                if (itemType.IsPrimitive)
                {
                    var value = element as JValue;
                    if (value != null)
                    {
                        list.Add(Convert.ChangeType(value.Value, itemType));
                    }
                }
                else if (itemType == typeof(string))
                {
                    list.Add(element.AsString());
                }
                else
                {
                    var item = CreateAndMap(itemType, element, culture, typeMappings, nestingLevel + 1);
                    list.Add(item);
                }
            }
            return list.ToArray(itemType);
        }

        public static IList BuildIEnumerable(Type type, JEnumerable<JToken> elements, CultureInfo culture, IEnumerable<TypeMapping> typeMappings, int nestingLevel)
        {
            typeMappings = typeMappings.ToArray();
            var itemType = type.GetGenericArguments().Single();
            var listType = typeof(List<>).MakeGenericType(itemType);
            var list = (IList)Activator.CreateInstance(listType);

            foreach (var element in elements)
            {
                if (itemType.IsPrimitive)
                {
                    var value = element as JValue;
                    if (value != null)
                    {
                        list.Add(Convert.ChangeType(value.Value, itemType));
                    }
                }
                else if (itemType == typeof (string))
                {
                    list.Add(element.AsString());
                }
                else
                {
                    var item = CreateAndMap(itemType, element, culture, typeMappings, nestingLevel + 1);
                    list.Add(item);
                }
            }

            return list;
        }

        static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> PropertyInfoCache = new Dictionary<Type, Dictionary<string, PropertyInfo>>();
        static readonly object PropertyInfoCacheLock = new object();
        static Dictionary<string, PropertyInfo> GetPropertiesForType(Type objType)
        {
            Dictionary<string, PropertyInfo> result;
            if (PropertyInfoCache.TryGetValue(objType, out result))
                return result;

            lock (PropertyInfoCacheLock)
            {
                if (PropertyInfoCache.TryGetValue(objType, out result))
                    return result;

                var properties = objType
                    .GetProperties()
                    .Where(p => p.CanWrite)
                    .Select(p =>
                    {
                        var attributes =
                            (JsonPropertyAttribute[])p.GetCustomAttributes(typeof(JsonPropertyAttribute), true);
                        return new
                        {
                            Name = attributes.Any() && attributes.Single().PropertyName  != null ? attributes.Single().PropertyName : p.Name,
                            Property = p
                        };
                    });

                return properties.ToDictionary(p => p.Name, p => p.Property);
            }
        }
    }
}
