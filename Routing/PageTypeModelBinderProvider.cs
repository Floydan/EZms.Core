using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using EZms.Core.Attributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Rentals.Infrastructure.Routing;

namespace EZms.Core.Routing
{
    public class PageTypeModelBinderProvider : IModelBinderProvider
    {
        public static ConcurrentDictionary<string, IEnumerable<Type>> TypeCache = new ConcurrentDictionary<string, IEnumerable<Type>>();
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            var types = TypeCache.GetOrAdd("types", s =>
            {
                return AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                    .Where(w => w.GetCustomAttributes(typeof(PageDataAttribute), true).Any())
                    .Select(w => w);
            });
            
            return types.Contains(context.Metadata.ModelType) && context.BindingInfo.PropertyFilterProvider == null ? 
                new BinderTypeModelBinder(typeof(PageEntityBinder)) : 
                null;
        }
    }
}
