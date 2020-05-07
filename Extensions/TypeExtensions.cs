using System;
using System.Reflection;
using EZms.Core.Attributes;

namespace EZms.Core.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsSubclassOfGeneric(this Type toCheck, Type generic) {
            while (toCheck != null && toCheck != typeof(object)) {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur) {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }
        public static PageDataAttribute GetPageDataValues(this Type type)
        {
            if (type == null) return null;

            var pageDataAttribute = type.GetCustomAttribute(typeof(PageDataAttribute)) as PageDataAttribute;
            return pageDataAttribute;
        }
    }
}
