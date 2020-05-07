using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EZms.Core.Attributes;

namespace EZms.Core.Models
{
    public interface IContent : IContentData, IComparable
    {
        [IgnoreProperty]
        int Id { get; set; }
        [IgnoreProperty]
        int? ParentId { get; set; }

        [Display(Name = "Name")]
        [IgnoreProperty]
        string Name { get; set; }

        [Display(Name = "Url slug", Description = "This is the url-part that will point to this content, it must be unique")]
        [IgnoreProperty]
        string UrlSlug { get; set; }

        [IgnoreProperty]
        string ContentTypeGuid { get; }

        [IgnoreProperty]
        Type ModelType { get; }

        [IgnoreProperty]
        string ModelAsJson { get; set; }

        [IgnoreProperty]
        int Order { get; set; }

        [IgnoreProperty]
        bool Published { get; set; }

        [IgnoreProperty]
        int PublishedVersion { get; set; }

        [IgnoreProperty]
        int SavedVersion { get; set; }
    }
}
