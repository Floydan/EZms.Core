using System;
using System.Collections.Generic;
using EZms.Core.Attributes;

namespace EZms.Core.Models
{
    public interface IContentData
    {
        [IgnoreProperty]
        DateTime CreatedAt { get; set; }
        [IgnoreProperty]
        DateTime UpdatedAt { get; set; }
        [IgnoreProperty]
        string AllowedGroupsAsJson { get; set; }
        [IgnoreProperty]
        List<string> AllowedGroups { get; set; }
    }
}
