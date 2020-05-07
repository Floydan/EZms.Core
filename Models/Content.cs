using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using EZms.Core.Attributes;
using EZms.Core.Extensions;
using EZms.Core.Helpers;
using EZms.Core.Routing;
using Newtonsoft.Json;

namespace EZms.Core.Models
{
    public class Content : IContent
    {        
        private static readonly ICachedContentTypeControllerMappings TypeMappings = ServiceLocator.Current.GetInstance<ICachedContentTypeControllerMappings>();

        [Key]
        [IgnoreProperty]
        public int Id { get; set; }

        [IgnoreProperty]
        public int? ParentId { get; set; }

        [IgnoreProperty]
        public virtual Content Parent { get; set; }

        [Display(Name = "Name")]
        [IgnoreProperty]
        public string Name { get; set; }

        [Display(Name = "Url slug", Description = "This is the url-part that will point to this content, it must be unique")]
        [IgnoreProperty]
        public string UrlSlug { get; set; }

        private string _contentTypeGuid;
        [IgnoreProperty]
        public string ContentTypeGuid
        {
            get => string.IsNullOrWhiteSpace(_contentTypeGuid) ? GetType().GetPageDataValues().Guid : _contentTypeGuid;
            set => _contentTypeGuid = string.IsNullOrWhiteSpace(value) ? GetType().GetPageDataValues().Guid : value;
        }

        private Type _modelType;
        [IgnoreProperty]
        public Type ModelType
        {
            get
            {
                if (_modelType != null) return _modelType;
                var type = TypeMappings.GetContentType(ContentTypeGuid);
                _modelType = !type.GenericTypeArguments.IsNullOrEmpty() ? type.GenericTypeArguments.First() : type;
                return _modelType;
            }
        }

        [IgnoreProperty]
        public virtual string ModelAsJson { get; set; }

        [IgnoreProperty]
        public int Order { get; set; }
        [IgnoreProperty]
        public bool Published { get; set; }
        [IgnoreProperty]
        public int PublishedVersion { get; set; }
        [IgnoreProperty]
        public int SavedVersion { get; set; }

        [IgnoreProperty]
        public DateTime CreatedAt { get; set; }
        [IgnoreProperty]
        public DateTime UpdatedAt { get; set; }

        [IgnoreProperty]
        public DateTime PublishedAt { get; set; }
        
        [IgnoreProperty]
        public string AllowedGroupsAsJson { get; set; }

        [IgnoreProperty]
        [NotMapped]
        public List<string> AllowedGroups
        {
            get => string.IsNullOrWhiteSpace(AllowedGroupsAsJson) ? null : JsonConvert.DeserializeObject<List<string>>(AllowedGroupsAsJson);
            set => AllowedGroupsAsJson = value.IsNullOrEmpty() ? null : JsonConvert.SerializeObject(value);
        }

        public int CompareTo(object obj)
        {
            if (!(obj is Content tocompare)) return 0;
            if (tocompare.Id != Id) return 0;
            if (tocompare.ParentId != ParentId) return 0;
            if (tocompare.Name != Name) return 0;
            if (tocompare.UrlSlug != UrlSlug) return 0;
            if (tocompare.ContentTypeGuid != ContentTypeGuid) return 0;
            if (tocompare.ModelAsJson != ModelAsJson) return 0;
            if (tocompare.Order != Order) return 0;
            if (tocompare.Published != Published) return 0;
            if (tocompare.PublishedVersion != PublishedVersion) return 0;
            if (tocompare.SavedVersion != SavedVersion) return 0;
            if (tocompare.CreatedAt != CreatedAt) return 0;
            if (tocompare.UpdatedAt != UpdatedAt) return 0;
            if (tocompare.PublishedAt != PublishedAt) return 0;
            if (!Equals(tocompare.AllowedGroups, AllowedGroups)) return 0;

            return 1;
        }
    }
}
