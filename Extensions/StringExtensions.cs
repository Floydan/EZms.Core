using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using EZms.Core.Attributes;
using EZms.Core.Helpers;
using EZms.Core.Routing;

namespace EZms.Core.Extensions
{
    public static class StringExtensions
    {

        public static string ReplaceDiacriticsAdv(this string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            var normalized = input.Normalize(NormalizationForm.FormD);

            var chars = normalized.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
            return (new string(chars).Normalize(NormalizationForm.FormC)).ToLowerInvariant();
        }

        public static string ReplaceDiacritics(this string input, bool allowSlash = false)
        {
            var tempBytes = Encoding.GetEncoding("ISO-8859-8").GetBytes(input);
            var asciiStr = Encoding.UTF8.GetString(tempBytes).Trim().ToLowerInvariant();
            
            asciiStr = Regex.Replace(asciiStr, allowSlash ? @"[^a-z0-9\-\/]" : @"[^a-z0-9\-]", "-");
            asciiStr = asciiStr.Replace(" ", "-");
            asciiStr = Regex.Replace(asciiStr, "(-){2,}", "-");

            return asciiStr.Trim('-');
        }

        public static string Capitalize(this string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;
            try
            {
                return char.ToUpper(input[0]) + input.Substring(1);
            }
            catch
            {
                return input;
            }
        }

        public static string ConvertToSafeAttributeValue(this string input)
        {
            input = input.ToLowerInvariant();
            return new Regex("[^a-z0-9-]*").Replace(input, string.Empty);
        }

        public static TimeSpan ToTimeSpan(this string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return TimeSpan.MinValue;
            return TimeSpan.Parse(input);
        }
        public static PageDataAttribute GetPageDataValues(this string guid)
        {
            if (string.IsNullOrEmpty(guid)) return null;
            var typeMappings = ServiceLocator.Current.GetInstance<ICachedContentTypeControllerMappings>();

            var type = typeMappings.GetContentType(guid);
            if (type == null) return null;

            var pageDataAttribute = type.GetCustomAttribute(typeof(PageDataAttribute)) as PageDataAttribute;

            if (pageDataAttribute == null && type.IsGenericType)
            {
                type = type.GenericTypeArguments.First();
                pageDataAttribute = type.GetCustomAttribute(typeof(PageDataAttribute)) as PageDataAttribute;
            }

            return pageDataAttribute;
        }
    }
}
