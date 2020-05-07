using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EZms.Core.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace EZms.Core.Routing
{
    public interface ICachedContentTypeControllerMappings
    {
        Type Get(string type);
        Type GetContentType(string guid);
        IEnumerable<Type> GetAllContentTypes();
        IEnumerable<Type> GetAll();
    }

    public class CachedContentTypeControllerMappings : ICachedContentTypeControllerMappings
    {
        private const string ControllersKey = "controllermappings";
        private const string ContentTypesKey = "contenttypemappings";
        private static readonly ConcurrentDictionary<string, Dictionary<string, Type>> TypeControllerMappingsCache = new ConcurrentDictionary<string, Dictionary<string, Type>>();

        public CachedContentTypeControllerMappings()
        {
            TypeControllerMappingsCache.GetOrAdd(ControllersKey, m =>
            {
                var dictionary = new Dictionary<string, Type>();
                var controllers = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                        .Where(w => w.IsClass && !w.IsAbstract &&
                                    w.IsSubclassOf(typeof(ControllerBase))
                                    && w.BaseType.IsGenericType).Select(w => w);

                foreach (var controller in controllers)
                {
                    dictionary.Add(controller.BaseType.GetGenericArguments().First().ToString(), controller);
                }

                return dictionary;
            });

            TypeControllerMappingsCache.GetOrAdd(ContentTypesKey, m =>
            {
                var pageTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                    .Where(w => w.GetCustomAttributes(typeof(PageDataAttribute), true).Any())
                    .Select(w => w);

                var dictionary = new Dictionary<string, Type>();

                foreach (var pageType in pageTypes)
                {
                    var pageData = (PageDataAttribute)pageType.GetCustomAttribute(typeof(PageDataAttribute));
                    dictionary.Add(pageData.Guid, pageType);
                }

                return dictionary;
            });
        }

        public Type Get(string type)
        {
            if (TypeControllerMappingsCache.TryGetValue(ControllersKey, out var dictionary))
            {
                if (dictionary.ContainsKey(type))
                {
                    return dictionary[type];
                }
            }

            return null;
        }

        public Type GetContentType(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return null;

            if (TypeControllerMappingsCache.TryGetValue(ContentTypesKey, out var dictionary))
            {
                if (dictionary.ContainsKey(guid))
                {
                    return dictionary[guid];
                }
            }

            return null;
        }

        public IEnumerable<Type> GetAllContentTypes()
        {
            return TypeControllerMappingsCache.TryGetValue(ContentTypesKey, out var dictionary) ? dictionary.Values.ToArray() : null;
        }

        public IEnumerable<Type> GetAll()
        {
            return TypeControllerMappingsCache.TryGetValue(ControllersKey, out var dictionary) ? dictionary.Values.ToArray() : null;
        }
    }
}
