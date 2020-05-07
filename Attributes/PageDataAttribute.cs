using System;

namespace EZms.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PageDataAttribute : Attribute
    {
        public string Name { get; set; }

        public string Guid { get; set; }
    }
}
