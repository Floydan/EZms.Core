using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using EZms.Core.Attributes;
using EZms.Core.Extensions;
using EZms.Core.Helpers;
using EZms.Core.Routing;
using Newtonsoft.Json;

namespace EZms.Core.Models
{
    public class ContentVersion : IContent
    {
        private static readonly ICachedContentTypeControllerMappings TypeMappings = ServiceLocator.Current.GetInstance<ICachedContentTypeControllerMappings>();

        public int Id { get; set; }
        public int ContentId { get; set; }
        public virtual Content Content { get; set; }
        public int? ParentId { get; set; }
        public string Name { get; set; }
        public string UrlSlug { get; set; }

        private string _contentTypeGuid;
        public string ContentTypeGuid
        {
            get => string.IsNullOrWhiteSpace(_contentTypeGuid) ? GetType().GetPageDataValues().Guid : _contentTypeGuid;
            set => _contentTypeGuid = string.IsNullOrWhiteSpace(value) ? GetType().GetPageDataValues().Guid : value;
        }

        private Type _modelType;
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

        public string ModelAsJson { get; set; }
        public int Order { get; set; }
        public bool Published { get; set; }
        public int PublishedVersion { get; set; }
        public int SavedVersion { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
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
            if (!(obj is ContentVersion tocompare)) return 0;
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
