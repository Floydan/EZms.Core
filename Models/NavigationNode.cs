using System;
using System.Collections.Generic;
using System.Linq;

namespace EZms.Core.Models
{
    public class NavigationNode
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public bool IsStartPage { get; set; }
        public bool IsPublished { get; set; }
        public ICollection<NavigationNode> Children { get; set; }
        public bool IsProduct { get; set; }
        public Type Type { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
    }
}
