using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ww.Utilities.Extensions
{
    [DebuggerStepThrough]
    public static class ObjectExtensions
    {
        public static object GetDefault(this Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        public static bool EqualsAnyOf<T>(this T obj, params T[] tests) where T : struct
        {
            return tests.Any(x => x.Equals(obj));
        }
        public static bool EqualsAnyOf<T>(this T obj, IEnumerable<T> tests) where T : struct
        {
            return tests.Any(x => x.Equals(obj));
        }

        public static T MemberwiseClone<T>(this T obj) where T : class
        {
            if (obj == null) return null;
            MethodInfo inst = obj.GetType().GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
            return inst == null ? null : (T) inst.Invoke(obj, null);
        }

        /// <summary>
        /// Copies the properties from one object implementing an interface to a new object of a different class that implements the exact same interface.
        /// </summary>
        public static TDestType CopyInterfaceProperties<TInterfaceType, TDestType>(this TInterfaceType source) where TInterfaceType : class where TDestType : class, new()
        {
            var dest = new TDestType();
            foreach (PropertyInfo prop in typeof(TInterfaceType).GetProperties())
            {
                prop.SetValue(dest, prop.GetValue(source, null), null);
            }
            return dest;
        }

        public static int ToInt(this object number)
        {
            if (number == null) return 0;
            int value;
            int.TryParse(number.ToString(), NumberStyles.Any & ~NumberStyles.AllowTrailingSign & ~NumberStyles.AllowExponent, CultureInfo.CurrentCulture, out value);
            return value;
        }

        public static long ToLong(this object number)
        {
            if (number == null) return 0;
            long value;
            long.TryParse(number.ToString(), NumberStyles.Any & ~NumberStyles.AllowTrailingSign & ~NumberStyles.AllowExponent, CultureInfo.CurrentCulture, out value);
            return value;
        }

        public static double ToDouble(this object number)
        {
            if (number == null) return 0;
            double value;
            double.TryParse(number.ToString(), NumberStyles.Any & ~NumberStyles.AllowTrailingSign & ~NumberStyles.AllowExponent, CultureInfo.CurrentCulture, out value);
            return value;
        }

        public static decimal ToDecimal(this object number)
        {
            return number.ToDecimal(out _);
        }

        public static decimal ToDecimal(this object number, out bool parsedSuccessfully)
        {
            parsedSuccessfully = false;
            if (number == null) return 0;
            parsedSuccessfully = decimal.TryParse(number.ToString(), NumberStyles.Any & ~NumberStyles.AllowTrailingSign & ~NumberStyles.AllowExponent, CultureInfo.CurrentCulture, out decimal value);
            return value;
        }

        public static DateTime ToDateTime(this object date, bool tryParse = true)
        {
            return date.ToDateTime(out _, tryParse: tryParse);
        }
        public static DateTime ToDateTime(this object date, out bool parsedSuccessfully, bool tryParse = true)
        {
            if (tryParse)
            {
                if (date == null)
                {
                    parsedSuccessfully = false;
                    return default(DateTime);
                }
                DateTime value;
                parsedSuccessfully = DateTime.TryParse(date.ToString(), out value);
                return value;
            }
            parsedSuccessfully = true;
            return DateTime.Parse(date.ToString());
        }

        public static TimeSpan ToTimeSpan(this object timeSpan, bool tryParse = true)
        {
            return timeSpan.ToTimeSpan(out _, tryParse: tryParse);
        }
        public static TimeSpan ToTimeSpan(this object timeSpan, out bool parsedSuccessfully, bool tryParse = true)
        {
            if (tryParse)
            {
                if (timeSpan == null)
                {
                    parsedSuccessfully = false;
                    return default(TimeSpan);
                }
                TimeSpan value;
                parsedSuccessfully = TimeSpan.TryParse(timeSpan.ToString(), out value);
                return value;
            }
            parsedSuccessfully = true;
            return TimeSpan.Parse(timeSpan.ToString());
        }

        public static T ToEnum<T>(this object input, bool ignoreCase = false, bool tryParse = true, bool parseLabel = false, bool parseAbbreviation = false, bool parseTag = false) where T : struct, IConvertible
        {
            return input.ToEnum<T>(out _, ignoreCase, tryParse, parseLabel, parseAbbreviation, parseTag);
        }
        public static T ToEnum<T>(this object input, out bool parsedSuccessfully, bool ignoreCase = false, bool tryParse = true, bool parseLabel = false, bool parseAbbreviation = false, bool parseTag = false) where T : struct, IConvertible
        {
            T value;
            Type type = typeof(T);
            string inputString = input == null ? "" : input.ToString();

            if (tryParse)
            {
                parsedSuccessfully = Enum.TryParse(inputString, ignoreCase, out value) && Enum.IsDefined(type, value);
                if (!parsedSuccessfully && (parseLabel || parseAbbreviation || parseTag))
                {
                    string inputTrimmed = inputString.Trim();
                    T?[] enumValues = Enum.GetValues(type).Cast<T?>().ToArray();
                    if (parseLabel)
                    {
                        ToEnum(inputTrimmed, enumValues, ignoreCase, EnumAttributeExtension.GetLabel, ref parsedSuccessfully, ref value);
                    }
                    if (!parsedSuccessfully && parseAbbreviation)
                    {
                        ToEnum(inputTrimmed, enumValues, ignoreCase, EnumAttributeExtension.GetAbbreviation, ref parsedSuccessfully, ref value);
                    }
                    if (!parsedSuccessfully && parseTag)
                    {
                        ToEnum(inputTrimmed, enumValues, ignoreCase, EnumAttributeExtension.GetTagAsString, ref parsedSuccessfully, ref value);
                    }
                }
                return value;
            }

            value = (T)Enum.Parse(type, inputString, ignoreCase);
            parsedSuccessfully = Enum.IsDefined(type, value);
            return value;
        }
        private static void ToEnum<T>(string inputTrimmed, T?[] enumValues, bool ignoreCase, Func<Enum, string> callbackFunc, ref bool parsedSuccessfully, ref T value) where T : struct, IConvertible
        {
            T? valueTemp = ignoreCase
                ? enumValues.FirstOrDefault(x => inputTrimmed.Equals(callbackFunc((Enum)(object)x), StringComparison.OrdinalIgnoreCase))
                : enumValues.FirstOrDefault(x => inputTrimmed == callbackFunc((Enum)(object)x));
            if (valueTemp != null)
            {
                value = valueTemp.Value;
                parsedSuccessfully = true;
            }
        }

        public static string ToJSON(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        public static string ToJSON(this object obj, JsonSerializerSettings settings)
        {
            return JsonConvert.SerializeObject(obj, settings);
        }

        public static string ToStringSafe(this object obj)
        {
            return obj == null ? "" : obj.ToString();
        }

        public static string ToTrimmedString(this object obj)
        {
            return obj.ToString().Trim();
        }

        public static string ToTrimmedStringSafe(this object obj)
        {
            return obj == null ? "" : obj.ToTrimmedString();
        }

        public static string ToCleanUpperString(this object obj)
        {
            return obj == null ? "" : obj.ToTrimmedString().ToUpper().Replace(" ","").StripNonAlphaNumericCharacters();
        }

        public static bool TryCast<T>(this object obj, out T result)
        {
            if (obj is T)
            {
                result = (T) obj;
                return true;
            }
            result = default(T);
            return false;
        }

        /// <summary>
        /// True when this is not an object of a class.
        /// </summary>
        public static bool IsSimpleType(this object obj, Type type = null)
        {
            if (type == null) type = obj.GetType();
            return type.IsValueType || type.IsPrimitive || Type.GetTypeCode(type) != TypeCode.Object;
        }

        public static bool IsNumericType(this object obj)
        {
            switch (Type.GetTypeCode(obj.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsFloatingPointType(this object obj)
        {
            switch (Type.GetTypeCode(obj.GetType()))
            {
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return true;
                default:
                    return false;
            }
        }

        public static string JsonValue(this JObject obj, string key)
        {
            var result = ""; //default to blank string if nothing is found

            foreach (var item in obj)
            {
                var token = item;

                if (token.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                {
                    result = token.Value.ToString(); //return the value found
                    break;
                }

                if (!obj[token.Key].Children().Any())
                    continue;

                var jt = obj[token.Key].ToString();

                if (!jt.StartsWith("["))
                {
                    result = JsonValue(JObject.Parse(jt), key);
                }
                else
                {
                    obj[token.Key].Children().ToList().ForEach(x =>
                    {
                        //only the first match will be returned
                        result = JsonValue(JObject.Parse(x.ToString()), key);
                    });
                }

                if (result != null)
                    break;

            }

            return result;
        }

        public static long ToLongNatural(this decimal dValue)
        {
            return (long)(dValue * 100);
        }

        public static bool HasProperty(this object obj, string propertyName)
        {
            return obj is System.Dynamic.ExpandoObject
                ? ((IDictionary<string, object>)obj).ContainsKey(propertyName)
                : obj.GetType().GetProperty(propertyName) != null;
        }

        public static bool HasMethod(this object obj, string methodName)
        {
            return obj.GetType().GetMethod(methodName) != null;
        }

        public static string ToBase64(this byte[] obj)
        {
            return Convert.ToBase64String(obj);
        }

        public static byte[] DecodeBase64(this string input)
        {
            return Convert.FromBase64String(input);
        }
    }
}