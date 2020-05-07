using System;

namespace EZms.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class IgnorePropertyAttribute : Attribute
    {
    }
}
